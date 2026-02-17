import { makeStyles, SearchBox, tokens } from "@fluentui/react-components";
import { SearchFiltersProps } from "../models/props/SearchFiltersProps";
import * as React from "react";

const useStyles = makeStyles({
  searchBox: {
    minWidth: '180px',
    maxWidth: '280px',
    flexGrow: 1 
  }
});

export const SearchFilters: React.FC<SearchFiltersProps> = ({
  onRowFilterChange,
  onColumnFilterChange,
  disabled,
  rowsLabel,
  columnsLabel
}) => {
  const styles = useStyles();
  
  return (
    <><SearchBox
      className={styles.searchBox}
      placeholder={"Search " + rowsLabel}
      onChange={(_, data) => onRowFilterChange(data.value)}
      disabled={disabled} />
      
      <SearchBox
        className={styles.searchBox}
        placeholder={"Search " + columnsLabel}
        onChange={(_, data) => onColumnFilterChange(data.value)}
        disabled={disabled} /></>
  );
};