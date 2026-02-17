/**
 * AddQuestionsPanelTabFilters.tsx
 * 
 * This component renders the Side panel tab filters, which determine the search results.
 */
import * as React from "react";
import { TabList, Tab, makeStyles } from "@fluentui/react-components";

const useStyles = makeStyles({
  tab: {
    borderRadius: "4px",
    padding: "6px 16px",
    border: "1px solid #ccc",
    marginRight: "8px",
    fontWeight: 500,
    cursor: "pointer",
    transition: "background-color 0.2s ease, color 0.2s ease",

    // selected tab
    '&[aria-selected="true"]::after': {
      backgroundColor: "#666", // selected underline color
      height: "2px",           // thickness of underline
    },

    // selected tab text
    '&[aria-selected="true"] span': {
      color: "white",
    },

    // unselected tab
    '&[aria-selected="true"]': {
      backgroundColor: "#666", // selected background
      color: "white !important",          // selected text
      border: "1px solid #666",
    },

    // underline for selected tab
    '&:not([aria-selected="true"])': {
      backgroundColor: "transparent",
      color: "#333",
    },
  },
});

interface PanelTabFiltersProps {
  selectedTab: string;
  onTabChange: (tab: string) => void;
  isScripter:boolean;
}

export const PanelTabFilters: React.FC<PanelTabFiltersProps> = ({ selectedTab, onTabChange, isScripter}) => {
  const styles = useStyles();

  return (
    <div style={{ padding: "0 16px", marginTop: "12px" }}>
      <TabList
        selectedValue={selectedTab}
        onTabSelect={(_, data) => {
          onTabChange(data.value as string);
        }}
      >
        <Tab className={styles.tab} value="questions">Questions</Tab>
        {!isScripter && (<Tab className={styles.tab} value="modules">Modules</Tab>)}       
        <Tab className={styles.tab} value="custom">Custom</Tab>
      </TabList>
    </div>
  );
};

