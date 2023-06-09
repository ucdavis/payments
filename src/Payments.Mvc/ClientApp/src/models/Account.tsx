export interface Account {
  id: number;
  name: string;
  description: string;
  isDefault: boolean;

  chart: string;
  account: string;
  subAccount: string;
  object: string;
  subObject: string;
  project: string;
  financialSegmentString: string;
}
