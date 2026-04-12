export type SqlReportPayload = {
  sql: string;
  server?: string;
  database?: string;
  title?: string;
  parameters?: Array<{
    tableId?: number;
    columnId?: number;
    columnName?: string;
    value?: string;
  }>;
  groupOrder?: Array<{
    order?: number;
    tableId?: number;
    columnId?: number;
    columnName?: string;
  }>;
};
