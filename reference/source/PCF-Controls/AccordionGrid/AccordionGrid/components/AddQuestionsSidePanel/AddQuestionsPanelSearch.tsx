/**
 * AddQuestionsPanelSearch.tsx
 * 
 * This component renders the Side panel search box.
 */
import * as React from "react";
import { SearchBox } from "@fluentui/react-components";
import { PanelSearchList } from "./AddQuestionsPanelSearchList";
import type { SearchType } from "../../types/AddQuestionsSidePanel/SearchType";

interface PanelSearchProps {
  items: SearchType[];
  selectedIds: string[];
  onResults?: (results: SearchType[]) => void;
  onSelectionChange?: (ids: string[]) => void;
 
}

export const PanelSearch: React.FC<PanelSearchProps> = ({ items, selectedIds, onResults, onSelectionChange }) => {
  const [value, setValue] = React.useState("");
  const [filtered, setFiltered] = React.useState<SearchType[]>([]);

  React.useEffect(() => {
    const lower = value.trim().toLowerCase();
    const results = lower
      ? items.filter(i =>
        i.label.toLowerCase().includes(lower) ||
        i.hiddenSearchFields?.some(f => f.toLowerCase().includes(lower))
      )
      : [];

    setFiltered(results);
    onResults?.(results);
  }, [value, items, onResults]);

  React.useEffect(() => {
    setValue("");
    setFiltered([]);
    onResults?.([]);
  }, [items, onResults]);

  return (
    <div style={{ padding: "0 16px" }}>
      <SearchBox
        placeholder="Search Questions and Modules"
        value={value}
        onChange={(_, data) => setValue(data?.value ?? "")}
        style={{ width: "100%" }}
      />
      <div style={{ marginTop: "12px" }}>
        <PanelSearchList
          results={filtered}
          searchText={value} // pass current search value
          selectedIds={selectedIds}
          onSelectionChange={onSelectionChange}
        />
      </div>
    </div>
  );
};
