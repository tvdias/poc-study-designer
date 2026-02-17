import * as React from 'react';
import {
  SearchBox,
  Button,
  makeStyles,
  tokens
} from '@fluentui/react-components';
import {
  Add16Regular,
  ArrowDownload16Regular
} from '@fluentui/react-icons';

interface SearchFiltersProps {
  rowFilter: string;
  columnFilter: string;
  onRowFilterChange: (value: string) => void;
  onColumnFilterChange: (value: string) => void;
  disabled?: boolean;
  loadedVisibleColumnCount: number;

  // Progressive loading props
  loadedRowCount: number;
  totalAvailableRows: number;
  isLoadingMoreRows: boolean;
  onLoadMoreRows: () => void;
  loadedColumnCount: number;
  totalAvailableColumns: number;
  isLoadingMoreColumns: boolean;
  onLoadMoreColumns: () => void;
  onLoadAllRows: () => void;
  onLoadAllColumns: () => void;
  parentEntityName?: string;
}

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
  searchBox: {
    minWidth: '180px',
    maxWidth: '280px',
    flexGrow: 1 
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

export const SearchFilters: React.FC<SearchFiltersProps> = ({
  rowFilter,
  columnFilter,
  onRowFilterChange,
  onColumnFilterChange,
  disabled = false,
  loadedRowCount,
  totalAvailableRows,
  isLoadingMoreRows,
  onLoadMoreRows,
  loadedColumnCount,
  totalAvailableColumns,
  isLoadingMoreColumns,
  onLoadMoreColumns,
  onLoadAllRows,
  onLoadAllColumns,
  loadedVisibleColumnCount,
  parentEntityName
}) => {
  const styles = useStyles();

  const canLoadMoreRows = loadedRowCount < totalAvailableRows;
  const canLoadMoreColumns = loadedColumnCount < totalAvailableColumns;

  const entityName = parentEntityName?.toLowerCase();
  const isManagedList = entityName === 'ktr_managedlist';

  const rowLabel = isManagedList ? 'Managed List Entities' : 'Questions';
  const rowSearchPlaceholder = isManagedList ? 'Search Managed List Entities' : 'Search Questions';

  return (
    <div className={styles.container}>

      {/* Left Section - Row Loading Controls */}
      <div className={styles.leftSection}>
        <span className={styles.statsText}>
          {rowLabel}: {loadedRowCount}/{totalAvailableRows}
        </span>

        {/*
          Hiding questions pagination - Loading all available
          <div className={styles.loadSection}>
            <Button
              className={styles.loadButton}
              appearance="secondary"
              icon={<Add16Regular />}
              onClick={onLoadMoreRows}
              disabled={!canLoadMoreRows || isLoadingMoreRows || disabled}
              title={isLoadingMoreRows ? 'Loading more questions...' : 'Load 10 more questions'}
            />

            {canLoadMoreRows && (
              <Button
                className={styles.loadAllButton}
                appearance="outline"
                icon={<ArrowDownload16Regular />}
                onClick={onLoadAllRows}
                disabled={isLoadingMoreRows || disabled}
                title="Load all remaining questions"
              />
            )}
          </div>
        */}

        <span className={styles.statsText}>
          Studies: {loadedVisibleColumnCount}/{totalAvailableColumns}
        </span>

        <div className={styles.loadSection}>
          <Button
            className={styles.loadButton}
            appearance="secondary"
            icon={<Add16Regular />}
            onClick={onLoadMoreColumns}
            disabled={!canLoadMoreColumns || isLoadingMoreColumns || disabled}
            title={isLoadingMoreColumns ? 'Loading more studies...' : 'Load 10 more studies'}
          />

          {/* Hide load all button
          {canLoadMoreColumns && (
            <Button
              className={styles.loadAllButton}
              appearance="outline"
              icon={<ArrowDownload16Regular />}
              onClick={onLoadAllColumns}
              disabled={isLoadingMoreColumns || disabled}
              title="Load all remaining studies"
            />
          )}
          */}
        </div>
      </div>

      {/* Right Section - Search Boxes */}
      <div className={styles.rightSection}>
        <SearchBox
          className={styles.searchBox}
          placeholder={rowSearchPlaceholder}
          value={rowFilter}
          onChange={(_, data) => onRowFilterChange(data.value)}
          disabled={disabled}
        />

        <SearchBox
          className={styles.searchBox}
          placeholder="Search Studies"
          value={columnFilter}
          onChange={(_, data) => onColumnFilterChange(data.value)}
          disabled={disabled}
        />
      </div>

    </div>
  );
};