// ===================== BASIC =====================

export type ID = number;

// ===================== COLUMN REF =====================

export interface ColumnRef {
  tableId: ID;
  columnId: ID;
  columnName: string;
}

// ===================== COLUMN =====================

export interface QueryColumn {
  show: boolean;

  column: ColumnRef;

  alias?: string | null;

  aggregate?: "COUNT" | "SUM" | "AVG" | "MIN" | "MAX" | null;

  criteria?: Condition | null;
}

// ===================== CONDITION =====================

export type Operator =
  | "="
  | "!="
  | ">"
  | "<"
  | ">="
  | "<="
  | "LIKE"
  | "IN"
  | "BETWEEN"
  | "IS NULL"
  | "IS NOT NULL";

export interface Condition {
  column: ColumnRef | string;
  operator: Operator;
  value?: any;
}
export interface RawCondition {
  type: "raw";
  sql: string;
}
export interface ConditionGroup {
  type: "AND" | "OR";
  conditions: (Condition | ConditionGroup | RawCondition)[];
}

// ===================== WHERE =====================

export type WhereClause = {
  mode: "builder";
  group: ConditionGroup;
};

// ===================== ORDER BY =====================

export interface OrderBy {
  column: ColumnRef | string;
  direction: "ASC" | "DESC";
}

// ===================== GROUP BY =====================

export interface GroupBy {
  column: ColumnRef | string;
}

// ===================== JOIN =====================

export interface Join {
  type: "INNER" | "LEFT" | "RIGHT" | "FULL"| "CROSS";
  tableId: ID;
  on: ConditionGroup;
  _meta?:{};
}

// ===================== TABLE =====================

export interface QueryTable {
  id: ID;

  tableName: string;
  alias?: string | null;

  columns: QueryColumn[];
}

// ===================== ROOT =====================

export interface QueryState {
  distinct?: boolean;

  tables?: QueryTable[];

  joins?: Join[];

  where?: WhereClause | null;
  having?: WhereClause | null;

  groupBy?: GroupBy[];
  orderBy?: OrderBy[];

  limit?: number;
  offset?: number;
}
