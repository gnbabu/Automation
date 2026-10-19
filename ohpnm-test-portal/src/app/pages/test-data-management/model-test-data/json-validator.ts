import { ITestDataFieldSchema, ITestDataFlowSchema, ITestDataSectionSchema } from '@interfaces';

export const MAX_JSON_BYTES = 1_048_576;
export const MAX_ROWS = 500;
const MAX_ERRORS = 50;

export interface JsonCheckResult {
  value?: unknown;
  errors: string[];
  conversions: string[];
}

export function rootSection(schema: ITestDataFlowSchema): ITestDataSectionSchema | undefined {
  return schema.sections.find((s) => !s.isList && s.sectionName === schema.flowName);
}

export function validateDocumentJson(schema: ITestDataFlowSchema, text: string): JsonCheckResult {
  const ctx = new Context();
  const parsed = parse(text, ctx);
  if (ctx.errors.length) return ctx.result();

  if (!isPlainObject(parsed)) {
    ctx.error(`The JSON must be an object - { "SectionName": ... } - matching the ${schema.flowName} model.`);
    return ctx.result();
  }

  const root = rootSection(schema);
  const sections = new Map(schema.sections.filter((s) => s !== root).map((s) => [s.sectionName, s]));
  const rootFields = new Map((root?.fields ?? []).map((f) => [f.name, f]));
  const allowed = [...sections.keys(), ...rootFields.keys()];

  const out: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(parsed)) {
    const section = sections.get(key);
    const field = rootFields.get(key);
    if (section) {
      const node = checkSection(section, value, key, ctx);
      if (node !== undefined) out[key] = node;
    } else if (field) {
      const v = checkValue(field, value, key, ctx);
      if (v.ok) out[key] = v.value;
    } else {
      ctx.error(unknown(key, key, allowed, `is not part of the ${schema.flowName} model`));
    }
  }
  return ctx.result(out);
}

export function validateSectionJson(schema: ITestDataFlowSchema, section: ITestDataSectionSchema, text: string): JsonCheckResult {
  const ctx = new Context();
  const parsed = parse(text, ctx);
  if (ctx.errors.length) return ctx.result();

  if (section === rootSection(schema)) {
    if (!isPlainObject(parsed)) {
      ctx.error(`${section.sectionName}: must be an object of the model's own fields - { "Field": value }.`);
      return ctx.result();
    }
    return ctx.result(checkRow(section, parsed, '', ctx));
  }
  return ctx.result(checkSection(section, parsed, section.sectionName, ctx));
}

class Context {
  errors: string[] = [];
  conversions: string[] = [];

  error(message: string): void {
    if (this.errors.length < MAX_ERRORS) this.errors.push(message);
    else if (this.errors.length === MAX_ERRORS) this.errors.push(`... more problems not shown (first ${MAX_ERRORS} listed).`);
  }

  result(value?: unknown): JsonCheckResult {
    return this.errors.length
      ? { errors: this.errors, conversions: [] }
      : { value, errors: [], conversions: this.conversions };
  }
}

function parse(text: string, ctx: Context): unknown {
  if (!text || !text.trim()) {
    ctx.error('The JSON is empty.');
    return undefined;
  }
  const bytes = new TextEncoder().encode(text).length;
  if (bytes > MAX_JSON_BYTES) {
    ctx.error(`The JSON is too large (${Math.round(bytes / 1024)} KB). The limit is ${MAX_JSON_BYTES / 1024} KB.`);
    return undefined;
  }
  try {
    const value = JSON.parse(text);
    findDuplicateKeys(text, ctx);
    return value;
  } catch (e: any) {
    ctx.error(`Invalid JSON: ${e?.message ?? e}`);
    return undefined;
  }
}

function findDuplicateKeys(text: string, ctx: Context): void {
  type Frame = { keys?: Set<string>; path: string; pending?: string; index: number; expectKey: boolean };
  const stack: Frame[] = [];
  const childPath = (): string => {
    const parent = stack[stack.length - 1];
    if (!parent) return '';
    return parent.keys ? join(parent.path, parent.pending ?? '') : `${parent.path}[${parent.index++}]`;
  };

  let i = 0;
  while (i < text.length) {
    const c = text[i];
    if (c === '{') {
      stack.push({ keys: new Set(), path: childPath(), index: 0, expectKey: true });
      i++;
    } else if (c === '[') {
      stack.push({ path: childPath(), index: 0, expectKey: false });
      i++;
    } else if (c === '}' || c === ']') {
      stack.pop();
      i++;
    } else if (c === ',') {
      const top = stack[stack.length - 1];
      if (top?.keys) top.expectKey = true;
      i++;
    } else if (c === '"') {
      let j = i + 1;
      while (j < text.length && text[j] !== '"') j += text[j] === '\\' ? 2 : 1;
      const str = JSON.parse(text.slice(i, j + 1)) as string;
      const top = stack[stack.length - 1];
      if (top?.keys && top.expectKey) {
        if (top.keys.has(str)) ctx.error(`${join(top.path, str)}: duplicate key - each key can appear only once.`);
        top.keys.add(str);
        top.pending = str;
        top.expectKey = false;
      } else {
        childPath();
      }
      i = j + 1;
    } else if (/[-0-9tfn]/.test(c)) {
      childPath();
      while (i < text.length && !/[,\]}\s]/.test(text[i])) i++;
    } else {
      i++;
    }
  }
}

function checkSection(section: ITestDataSectionSchema, node: unknown, path: string, ctx: Context): unknown {
  if (section.isList) {
    if (!Array.isArray(node)) {
      ctx.error(`${path}: must be an array of rows - [ { ... }, { ... } ] - because ${section.sectionName} holds multiple rows.`);
      return undefined;
    }
    if (node.length > MAX_ROWS) {
      ctx.error(`${path}: has ${node.length} rows; the limit is ${MAX_ROWS}.`);
      return undefined;
    }
    const rows: unknown[] = [];
    node.forEach((item, i) => {
      if (!isPlainObject(item)) ctx.error(`${path}[${i}]: each row must be an object - { "Field": value }.`);
      else rows.push(checkRow(section, item, `${path}[${i}]`, ctx));
    });
    return rows;
  }
  if (!isPlainObject(node)) {
    ctx.error(
      `${path}: must be an object - { "Field": value }` +
        (Array.isArray(node) ? ` - not an array, because ${section.sectionName} holds a single row.` : '.'),
    );
    return undefined;
  }
  return checkRow(section, node, path, ctx);
}

function checkRow(section: ITestDataSectionSchema, obj: Record<string, unknown>, path: string, ctx: Context): Record<string, unknown> {
  const fields = new Map(section.fields.map((f) => [f.name, f]));
  const row: Record<string, unknown> = {};
  for (const [key, value] of Object.entries(obj)) {
    const fieldPath = join(path, key);
    const field = fields.get(key);
    if (!field) {
      ctx.error(unknown(fieldPath, key, [...fields.keys()], `is not a field of ${section.sectionName}`));
      continue;
    }
    const v = checkValue(field, value, fieldPath, ctx);
    if (v.ok) row[key] = v.value;
  }
  return row;
}

function checkValue(field: ITestDataFieldSchema, value: unknown, path: string, ctx: Context): { ok: boolean; value?: unknown } {
  const raw = JSON.stringify(value);
  const fail = (message: string) => {
    ctx.error(`${path}: ${raw} ${message}`);
    return { ok: false };
  };
  const converted = (to: unknown) => {
    ctx.conversions.push(`${path}: ${raw} → ${JSON.stringify(to)}`);
    return { ok: true, value: to };
  };

  if (value === null) {
    if (field.nullable) return { ok: true, value: null };
    return fail(`isn't allowed - this ${describe(field)} field can't be empty in the model.`);
  }
  if (typeof value === 'object') {
    ctx.error(`${path}: must be a single value, not ${Array.isArray(value) ? 'an array' : 'an object'}.`);
    return { ok: false };
  }

  const range = field.min != null && field.max != null ? ` (allowed ${field.min}..${field.max})` : '';
  const inRange = (n: number) => (field.min == null || n >= field.min) && (field.max == null || n <= field.max);

  switch (field.type) {
    case 'integer':
      if (typeof value === 'number') {
        if (!Number.isInteger(value)) return fail(`is not a valid whole number${range}.`);
        return inRange(value) ? { ok: true, value } : fail(`is out of range${range}.`);
      }
      if (typeof value === 'string' && /^[+-]?\d+$/.test(value)) {
        const n = Number(value);
        return inRange(n) ? converted(n) : fail(`is out of range${range}.`);
      }
      return fail(`is not a valid whole number${range}.`);

    case 'decimal':
      if (typeof value === 'number') return { ok: true, value };
      if (typeof value === 'string' && /^[+-]?(\d+\.?\d*|\.\d+)$/.test(value)) return converted(Number(value));
      return fail('is not a valid number (use a dot for decimals, e.g. 150.50).');

    case 'date':
      if (typeof value === 'string') {
        const m = /^(\d{4})-(\d{2})-(\d{2})(T(\d{2}):(\d{2}):(\d{2}))?$/.exec(value);
        if (m && isRealDate(+m[1], +m[2], +m[3]) && (!m[4] || (+m[5] < 24 && +m[6] < 60 && +m[7] < 60))) {
          const normalized = m[4] && m[5] === '00' && m[6] === '00' && m[7] === '00' ? value.substring(0, 10) : value;
          return { ok: true, value: normalized };
        }
      }
      return fail('is not a valid date - use "yyyy-MM-dd" (e.g. "1985-06-12").');

    case 'boolean':
      if (typeof value === 'boolean') return { ok: true, value };
      if (typeof value === 'string' && /^(true|false)$/i.test(value)) return converted(value.toLowerCase() === 'true');
      return fail('is not valid - use true or false.');

    case 'enum': {
      const options = field.options ?? [];
      if (typeof value === 'string') {
        if (options.includes(value)) return { ok: true, value };
        const match = options.find((o) => o.toLowerCase() === value.toLowerCase());
        if (match) return converted(match);
      }
      return fail(`is not one of: ${options.join(', ')}.`);
    }

    default:
      if (typeof value === 'string') return { ok: true, value };
      if (typeof value === 'number' || typeof value === 'boolean') return converted(String(value));
      return fail('is not valid text.');
  }
}

function isRealDate(y: number, m: number, d: number): boolean {
  const dt = new Date(Date.UTC(y, m - 1, d));
  return dt.getUTCFullYear() === y && dt.getUTCMonth() === m - 1 && dt.getUTCDate() === d;
}

function describe(field: ITestDataFieldSchema): string {
  return field.type === 'integer' ? 'whole-number' : field.type === 'decimal' ? 'number' : field.type === 'boolean' ? 'true/false' : field.type;
}

function unknown(path: string, key: string, allowed: string[], message: string): string {
  const hint = allowed.find((a) => a.toLowerCase() === key.toLowerCase());
  return hint
    ? `${path}: "${key}" ${message} - did you mean "${hint}"? (names are case-sensitive)`
    : `${path}: "${key}" ${message}.`;
}

function join(path: string, name: string): string {
  return path ? `${path}.${name}` : name;
}

function isPlainObject(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
