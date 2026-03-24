export interface DatabaseMetadata {
  tables: TableMetadata[];
}

export interface TableMetadata {
  objectId: number;
  tableName: string;
  checked: boolean;

  columns: ColumnMetadata[];
  foreignKeys: ForeignKeyMetadata[];
}

export interface ColumnMetadata {
  checked: boolean
  objectId: number;
  columnId: number;
  columnName: string;
  dataType: string;
}

export interface ForeignKeyMetadata {
  foreignKeyName: string;

  parentObjectId: number;
  parentTable: string;
  parentColumnId: number;
  parentColumn: string;

  referencedObjectId: number;
  referencedTable: string;
  referencedColumnId: number;
  referencedColumn: string;
}