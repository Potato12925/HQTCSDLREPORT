export type SqlReportPayload = {
  sql: string;
  server?: string;
  database?: string;
  title?: string;

  parameters?: Record<string, string>;

  groupOrder?: string[];
};
