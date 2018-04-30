export function distinct(array, selector) {
  const map = {};
  for (let i = 0; i < array.length; i++) {
    const item = array[i];
    const key = selector(item);
    map[key] = item;
  }

  return Object.keys(map).map(k => map[k]);
}
