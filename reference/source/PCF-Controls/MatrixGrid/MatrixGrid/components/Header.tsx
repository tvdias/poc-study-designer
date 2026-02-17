import { makeStyles, tokens, Button } from "@fluentui/react-components";
import { HeaderProps } from "../models/props/HeaderProps";
import * as React from "react";
import { SearchFilters } from "./SearchFilters";
import { DropdownFilter } from "./DropdownFilter";
import { ArrowClockwiseRegular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    padding: '12px 0',
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    marginBottom: '16px',
    gap: '16px'
  },
  leftSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px'
  },
  rightSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px'
  },
  loadButton: {
    minWidth: '32px',
    width: '32px',
    height: '32px'
  },
  loadAllButton: {
    minWidth: '28px',
    width: '28px',
    height: '28px'
  },
  statsText: {
    fontSize: tokens.fontSizeBase100,
    color: tokens.colorNeutralForeground2,
    marginLeft: '4px',
    marginRight: '8px',
    minWidth: '70px'
  },
  loadSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '4px'
  }
});

export const Header: React.FC<HeaderProps> = ({
    onRowFilterChange,
    onColumnFilterChange,
    disabled,
    rowsLabel,
    columnsLabel,
    loadedRowsCount,
    loadedColumnsCount,
    onDropdownFilterChange,
    dropdownItems,
    selectedDropdownValue,
    onRefresh
}) => {
    const styles = useStyles();

    /* No Pagination implementated yet */
    const totalAvailableRows = loadedRowsCount;
    const totalAvailableColumns = loadedColumnsCount;
    
    console.log('SearchFilters - loadedRowsCount: ', loadedRowsCount);
    console.log('SearchFilters - loadedColumnsCount: ', loadedColumnsCount);

    // Refresh dataset
    const refreshData = () => {
      onRefresh();
    };
    
    return (
        <div className={styles.container}>
          {/* Left Section - Row/Columns count */}
          <div className={styles.leftSection}>
            <span className={styles.statsText}>
              {rowsLabel}: {loadedRowsCount}/{totalAvailableRows}
            </span>
    
            <span className={styles.statsText}>
              {columnsLabel}: {loadedColumnsCount}/{totalAvailableColumns}
            </span>
          </div>
    
          {/* Right Section - Dropdown & Search Boxes */}
          <div className={styles.rightSection}>
          <Button
                appearance="secondary"
                icon={<ArrowClockwiseRegular/>}
                onClick={refreshData}
              >
                Refresh
              </Button>
            <DropdownFilter
              onDropdownFilterChange={onDropdownFilterChange}
              dropdownItems={dropdownItems}
              selectedValue={selectedDropdownValue}
            ></DropdownFilter>
            
            <SearchFilters
              onRowFilterChange={onRowFilterChange}
              onColumnFilterChange={onColumnFilterChange}
              disabled={disabled}
              rowsLabel={rowsLabel}
              columnsLabel={columnsLabel}
            />
          </div>
    
        </div>
      );
}