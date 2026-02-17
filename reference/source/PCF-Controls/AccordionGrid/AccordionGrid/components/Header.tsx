import { makeStyles, Select, Subtitle1, Button } from "@fluentui/react-components";
import * as React from "react";
import { HeaderProps } from "../models/props/HeaderProps";
import { ViewType } from "../types/ViewType";
import { SearchFilter } from './SearchFilter';
import { DeleteRegular } from "@fluentui/react-icons";
import { RemoveModulesPanelContainer } from "./RemoveModulesSidePanel/RemoveModulesPanelContainer";
import { AddCircleRegular } from '@fluentui/react-icons';
import { AddQuestionsPanelContainer } from './AddQuestionsSidePanel/AddQuestionsPanelContainer';
import { ArrowClockwiseRegular } from '@fluentui/react-icons';

const useStyles = makeStyles({
  headerContainer: {
    display: "flex",
    alignItems: 'center',
    padding: 'var(--spacingHorizontalM) var(--spacingHorizontalM) var(--spacingHorizontalM) var(--spacingHorizontalMNudge)',
  },
  leftPanel: {
    flex: 1,
  },
  filterPanel: {
    display: 'flex',
    gap: 'var(--spacingHorizontalM)',
    justifyContent: 'space-between'
  },
  removeButton: {
    justifyContent: "flex-start",
  }
});

export const Header: React.FC<HeaderProps> = ({
  context,
  view,
  updateView,
  onSearch,
  isReadOnly,
  rows,
  entityName, 
  isScripter
}) => {
  const styles = useStyles();

  //Remove module side panel state
  const [isRemovePanelOpen, setRemovePanelOpen] = React.useState(false);
  //Add question/module side panel state
  const [isAdditionPanelOpen, setAdditionPanelOpen] = React.useState(false);

  function handleViewChange(newValue: string) {
    const state = newValue as ViewType;
    updateView(state);
  }

  function handleRemoveModules() {
    setRemovePanelOpen(true);
  }

  // Refresh dataset
  const refreshData = () => {
    context.parameters.gridDataSet.refresh();
  };


  return (
    <>
      <div className={styles.headerContainer}>
        <div className={styles.leftPanel}>
          <Subtitle1>Questions</Subtitle1>
        </div>

        <div className={styles.filterPanel}>
          <Button
                appearance="secondary"
                icon={<ArrowClockwiseRegular/>}
                onClick={refreshData}
              >
                Refresh
              </Button>
          {(!isReadOnly || isScripter)&& (
            <>
              <Button
                appearance="transparent"
                icon={<AddCircleRegular />}
                onClick={(e) => {
                  e.stopPropagation();
                  setAdditionPanelOpen(true);
                }}
                disabled={view === ViewType.Inactive}
              >
               </Button></>)}
          {!isReadOnly &&
            (<>
              <Button
                appearance="secondary"
                icon={<DeleteRegular />}
                className={styles.removeButton}
                onClick={handleRemoveModules}
                disabled={view === ViewType.Inactive}
              >
                Remove Modules
              </Button>
            </>

          )}

          <Select
            appearance="underline"
            size="large"
            value={view}
            onChange={(e, data) => handleViewChange(data.value)}
          >
            <option value="active">Active Questions</option>
            <option value="inactive">Inactive Questions</option>
            <option value="all">All Questions</option>
            <option value="dummy">Dummy Questions</option>
          </Select>

          <SearchFilter context={context} onSearch={onSearch} />
        </div>
      </div>

      {/* Remove Module Side panel */}
      <RemoveModulesPanelContainer
        isOpen={isRemovePanelOpen}
        onClose={() => setRemovePanelOpen(false)}
        rows={rows}
        context={context}
        isReadOnly={isReadOnly}
        entityName={entityName}
      />

      {/* Add questions Side panel */}
      <AddQuestionsPanelContainer
        isOpen={isAdditionPanelOpen}
        onClose={() => setAdditionPanelOpen(false)}
        row={undefined}
        existingRows={rows} //pass all existing rows if any(questions) on project
        onRefresh={() => refreshData()}
        addFromHeader={true}
        context={context}
        isScripter={isScripter}
      />
    </>
  );
};
