import * as React from 'react';
import {
  Button,
  Spinner,
  makeStyles,
  tokens,
  Text
} from '@fluentui/react-components';
import {
  Add20Regular,
  ArrowDownload20Regular
} from '@fluentui/react-icons';

import { ProgressiveLoaderProps } from '../types/MatrixTypes';

const useStyles = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
    padding: '16px',
    backgroundColor: tokens.colorNeutralBackground2,
    borderRadius: tokens.borderRadiusMedium,
    border: `1px solid ${tokens.colorNeutralStroke2}`
  },
  section: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    gap: '12px'
  },
  loadingSection: {
    display: 'flex',
    alignItems: 'center',
    gap: '16px'
  },
  statsText: {
    fontSize: tokens.fontSizeBase200,
    color: tokens.colorNeutralForeground2,
    fontWeight: tokens.fontWeightMedium
  },
  loadMoreButton: {
    minWidth: '120px'
  },
  loadAllButton: {
    minWidth: '100px'
  },
  progressInfo: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px'
  },
  divider: {
    height: '1px',
    backgroundColor: tokens.colorNeutralStroke2,
    margin: '8px 0'
  }
});

export const ProgressiveLoader: React.FC<ProgressiveLoaderProps> = ({
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
  disabled = false
}) => {
  const styles = useStyles();

  const canLoadMoreRows = loadedRowCount < totalAvailableRows;
  const canLoadMoreColumns = loadedColumnCount < totalAvailableColumns;

  return (
    <div className={styles.container}>
      
      {/* Row Loading Section */}
      <div className={styles.section}>
        <div className={styles.progressInfo}>
          <Text className={styles.statsText}>
             Questions: {loadedRowCount} / {totalAvailableRows} loaded
          </Text>
        </div>
        
        <div className={styles.loadingSection}>
          {isLoadingMoreRows && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Spinner size="tiny" />
              <Text size={200}>Loading...</Text>
            </div>
          )}
          
          <Button
            className={styles.loadMoreButton}
            appearance="secondary"
            icon={<Add20Regular />}
            onClick={onLoadMoreRows}
            disabled={!canLoadMoreRows || isLoadingMoreRows || disabled}
          >
            Load 10 More
          </Button>
          
          {canLoadMoreRows && (
            <Button
              className={styles.loadAllButton}
              appearance="outline"
              icon={<ArrowDownload20Regular />}
              onClick={onLoadAllRows}
              disabled={isLoadingMoreRows || disabled}
              size="small"
            >
              Load All
            </Button>
          )}
        </div>
      </div>

      {/* Divider */}
      <div className={styles.divider} />

      {/* Column Loading Section */}
      <div className={styles.section}>
        <div className={styles.progressInfo}>
          <Text className={styles.statsText}>
             Studies: {loadedColumnCount} / {totalAvailableColumns} loaded
          </Text>
        </div>
        
        <div className={styles.loadingSection}>
          {isLoadingMoreColumns && (
            <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Spinner size="tiny" />
              <Text size={200}>Loading...</Text>
            </div>
          )}
          
          <Button
            className={styles.loadMoreButton}
            appearance="secondary"
            icon={<Add20Regular />}
            onClick={onLoadMoreColumns}
            disabled={!canLoadMoreColumns || isLoadingMoreColumns || disabled}
          >
            Load 10 More
          </Button>
          
          {canLoadMoreColumns && (
            <Button
              className={styles.loadAllButton}
              appearance="outline"
              icon={<ArrowDownload20Regular />}
              onClick={onLoadAllColumns}
              disabled={isLoadingMoreColumns || disabled}
              size="small"
            >
              Load All
            </Button>
          )}
        </div>
      </div>

      {/* Quick Stats */}
      <div style={{ 
        fontSize: '11px', 
        color: tokens.colorNeutralForeground3,
        textAlign: 'center',
        marginTop: '4px'
      }}>
        Showing {loadedRowCount * loadedColumnCount} of {totalAvailableRows * totalAvailableColumns} total possible assignments
      </div>

    </div>
  );
};