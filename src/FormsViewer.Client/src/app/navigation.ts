export interface NavigationModel {
  title: string;
  url?: string;
  path: string;
  hasChildren: boolean;
  children: NavigationModel[];
}

export interface NavigationWrapper {
  items: Array<NavigationModel>;
}

export class ItemModel {
  DisplayName: string;
  Name: string;
  Path: string;
  HasChildren: boolean;
  ID: string;
  Icon: string;
  Children: ItemModel[];
  State: any;
}

export class ItemWebApiResponse {
  statusCode: number;
  result: ItemWebApiResult;
}

export class ItemWebApiResult {
  TotalCount: number;
  ResultCount: number;
  Items: ItemModel[];
}