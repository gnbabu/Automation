// Shared badge color-pairing helper.
//
// Bootstrap's bg-* utility classes only set background-color, not text color, and this
// app's shared `.status-pill` class (styles.css) doesn't set a text color either - so
// every status/lifecycle/sign-off badge that returned just a bg-* class ended up with
// illegible dark/black text on dark backgrounds (Completed/bg-primary, Active/bg-success,
// Rejected/bg-danger, Approved/bg-success, etc). This always pairs a background class
// with the correct contrasting text color, so callers never have to remember to do it
// themselves.
export function pairBadgeTextColor(bgClass: string): string {
  const darkBackgrounds = [
    'bg-primary',
    'bg-success',
    'bg-danger',
    'bg-secondary',
    'bg-dark',
  ];
  return darkBackgrounds.includes(bgClass) ? `${bgClass} text-white` : `${bgClass} text-dark`;
}
