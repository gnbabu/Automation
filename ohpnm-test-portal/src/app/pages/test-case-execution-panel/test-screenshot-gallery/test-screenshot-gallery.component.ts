import { Component, ElementRef, HostListener, Input, OnChanges, SimpleChanges, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ITestScreenshot } from '@interfaces';

declare var bootstrap: any;

@Component({
  selector: 'app-test-screenshot-gallery',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './test-screenshot-gallery.component.html',
  styleUrls: ['./test-screenshot-gallery.component.css'],
})
export class TestScreenshotGalleryComponent implements OnChanges {
  @Input() screenshots: ITestScreenshot[] = [];

  @ViewChild('galleryModal', { static: false }) galleryModal!: ElementRef;
  @ViewChild('thumbStrip', { static: false }) thumbStrip?: ElementRef<HTMLElement>;
  modalInstance: any;

  index = 0;
  actualSize = false;
  private isOpen = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['screenshots']) {
      this.index = 0;
      this.actualSize = false;
    }
  }

  get current(): ITestScreenshot | undefined {
    return this.screenshots?.[this.index];
  }

  get title(): string {
    const first = this.screenshots?.[0];
    if (!first) return 'Screenshots';
    const name = [first.className, first.methodName].filter((p) => !!p).join(' / ');
    return name ? `Screenshots - ${name}` : 'Screenshots';
  }

  open() {
    if (!this.galleryModal) return;
    this.index = 0;
    this.actualSize = false;
    const modalEl = this.galleryModal.nativeElement;
    this.modalInstance = bootstrap.Modal.getOrCreateInstance(modalEl);
    modalEl.addEventListener('hidden.bs.modal', () => (this.isOpen = false), { once: true });
    this.modalInstance.show();
    this.isOpen = true;
  }

  close() {
    this.modalInstance?.hide();
  }

  select(i: number) {
    if (!this.screenshots?.length) return;
    this.index = (i + this.screenshots.length) % this.screenshots.length;
    this.actualSize = false;
    this.scrollThumbIntoView();
  }

  prev() {
    this.select(this.index - 1);
  }

  next() {
    this.select(this.index + 1);
  }

  toggleSize() {
    this.actualSize = !this.actualSize;
  }

  @HostListener('document:keydown', ['$event'])
  onKey(event: KeyboardEvent) {
    if (!this.isOpen || (this.screenshots?.length ?? 0) < 2) return;
    if (event.key === 'ArrowLeft') {
      this.prev();
      event.preventDefault();
    } else if (event.key === 'ArrowRight') {
      this.next();
      event.preventDefault();
    }
  }

  isFailure(shot: ITestScreenshot | undefined): boolean {
    return !!shot?.caption && /^failure/i.test(shot.caption);
  }

  label(shot: ITestScreenshot | undefined, i: number): string {
    const caption = (shot?.caption ?? '').trim();
    if (!caption || /^screenshot[_ ]?\d+$/i.test(caption)) return `Step ${i + 1}`;
    if (this.isFailure(shot)) {
      const detail = caption.replace(/^failure[_ :-]*/i, '').replace(/_/g, ' ').trim();
      return detail ? `Failure - ${detail}` : 'Failure';
    }
    return caption.replace(/_/g, ' ');
  }

  openFullSize() {
    const url = this.toBlobUrl(this.current);
    if (url) window.open(url, '_blank', 'noopener');
  }

  download() {
    const shot = this.current;
    const url = this.toBlobUrl(shot);
    if (!url || !shot) return;
    const a = document.createElement('a');
    a.href = url;
    a.download = `${this.label(shot, this.index).replace(/[^\w\- ]+/g, '').replace(/\s+/g, '_') || 'screenshot'}.png`;
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 10000);
  }

  private toBlobUrl(shot: ITestScreenshot | undefined): string | null {
    const data = shot?.screenshot ?? '';
    const match = /^data:([^;]+);base64,(.*)$/.exec(data);
    if (!match) return data || null;
    const bytes = atob(match[2]);
    const buffer = new Uint8Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) buffer[i] = bytes.charCodeAt(i);
    return URL.createObjectURL(new Blob([buffer], { type: match[1] }));
  }

  private scrollThumbIntoView() {
    setTimeout(() => {
      const strip = this.thumbStrip?.nativeElement;
      const active = strip?.querySelector<HTMLElement>('.thumb.active');
      active?.scrollIntoView({ block: 'nearest', inline: 'center', behavior: 'smooth' });
    });
  }
}
