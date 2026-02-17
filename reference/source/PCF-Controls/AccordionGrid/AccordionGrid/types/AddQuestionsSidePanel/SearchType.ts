export interface SearchType {
  id: string;
  label: string; // text shown in UI (search + accordion header)
  type: "question" | "module";
  details?: { [key: string]: string }; // key-value pairs for accordion content
  hiddenSearchFields?: string[]; // extra fields searchable but not shown
}
