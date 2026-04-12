// ===================== BASIC =====================

export type ID = number;

// ===================== COLUMN REF =====================
export type ColumnDataType = "number" | "string" | "boolean" | "date";

export interface ColumnRef {
  tableId: ID;
  columnId: ID;
  columnName: string;
  dataType?: ColumnDataType;
}

// ===================== COLUMN =====================

export interface QueryColumn {
  show: boolean;

  column: ColumnRef;

  alias?: string | null;

  aggregate?: "COUNT" | "SUM" | "AVG" | "MIN" | "MAX" | null;

  criteria?: Condition | null;

  parameterReport?: boolean;

  groupReport?: boolean;
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

export type HavingCondition =
  | {
      type: "aggregate";
      fn: "COUNT" | "SUM" | "AVG" | "MIN" | "MAX";
      column: ColumnRef;
      operator: Operator;
      value?: any;
    }
  | {
      type: "group_column";
      column: ColumnRef;
      operator: Operator;
      value?: any;
    }
  | {
      type: "alias";
      alias: string;
      aliasColumn?: ColumnRef;
      operator: Operator;
      value?: any;
    };
    
export interface HavingConditionGroup {
  type: "AND" | "OR";
  conditions: (HavingCondition | HavingConditionGroup)[];
}
// ===================== WHERE/HAVING =====================

export type WhereClause = {
  mode: "builder";
  group: ConditionGroup;
};

export type HavingClause = {
  mode: "builder";
  group: HavingConditionGroup;
}

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
  type: "INNER" | "LEFT" | "RIGHT" | "FULL" | "CROSS";
  fromTableId: ID;
  toTableId: ID;
  on: ConditionGroup;
  _meta?: {
    key: string;
    auto?: boolean;
  };
}

// ===================== TABLE =====================

export interface QueryTable {
  id: ID;

  tableName: string;
  alias?: string | null;

  columns: QueryColumn[];
}

//====================== SELECT * =====================

export interface SelectStar {
  alias?: string | null;
}

// ===================== ROOT =====================

export interface QueryState {
  distinct?: boolean;
  star?: SelectStar;
  tables?: QueryTable[];

  from?: QueryTable;
  joins?: Join[];

  where?: WhereClause | null;
  having?: HavingClause | null;

  groupBy?: GroupBy[];
  orderBy?: OrderBy[];
}
