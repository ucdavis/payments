export function distinct<T>(array: T[], selector: (item: T) => any): T[] {
  const map = {};
  for (const item of array) {
    const key = selector(item);
    map[key] = item;
  }

  return Object.keys(map).map(k => map[k]);
}
