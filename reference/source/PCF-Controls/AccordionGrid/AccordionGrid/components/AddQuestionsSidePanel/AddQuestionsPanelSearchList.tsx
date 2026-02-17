/**
 * AddQuestionsPanelSearchList.tsx
 * 
 * This component renders the Side panel search box results with accordion and checkboxes.
 */
import * as React from "react";
import {
  Accordion,
  AccordionItem,
  AccordionHeader,
  AccordionPanel,
  Checkbox,
  makeStyles,
  shorthands,
} from "@fluentui/react-components";
import type { SearchType } from "../../types/AddQuestionsSidePanel/SearchType";

const useStyles = makeStyles({
  listWrapper: {
    display: "flex",
    flexDirection: "column",
    height: "100%", // take full available height
  },
  list: {
    flex: 1,             // take remaining space
    overflowY: "auto",   // scroll if content exceeds
    backgroundColor: "#ffffff", // white background
    borderRadius: "4px",
    paddingBottom: "16px",
  },
  rowHeader: {
    display: "flex",
    alignItems: "center",
    ...shorthands.padding("4px", "0"),
    gap: "8px",
  },
  panelContent: {
    paddingLeft: "32px",
    paddingBottom: "8px",
    color: "#333",
  },
  detailRow: {
    marginBottom: "8px",
    "& strong": {
      fontWeight: "bold",
    },
  },
});

interface PanelSearchListProps {
  results: SearchType[];
  searchText: string;
  selectedIds: string[];
  onSelectionChange?: (ids: string[]) => void;
}

export const PanelSearchList: React.FC<PanelSearchListProps> = ({
  results,
  searchText,
  selectedIds,
  onSelectionChange,
}) => {
  const styles = useStyles();

  const toggle = (id: string) => {
    const next = selectedIds.includes(id)
      ? selectedIds.filter((x) => x !== id)
      : [...selectedIds, id];
    onSelectionChange?.(next);
  };

  return (
    <div className={styles.listWrapper}>
      {results.length === 0 && searchText.trim() !== "" ? (
        <div style={{ color: "#666", fontStyle: "italic" }}>
          No results matching your Search. Please try again.
        </div>
      ) : results.length > 0 ? (
        <div className={styles.list}>
          <Accordion multiple collapsible>
            {results.map((item) => {
              const isChecked = selectedIds.includes(item.id);

              return (
                <AccordionItem key={item.id} value={item.id}>
                  <AccordionHeader
                    className={styles.rowHeader}
                    expandIconPosition="end"
                  >
                    <div
                      style={{ display: "flex", alignItems: "center", width: "100%" }}
                      onClick={(e) => {
                        const target = e.target as HTMLElement;
                        if (target.closest(".fui-AccordionHeader__expandIcon")) return;
                        toggle(item.id);
                      }}
                    >
                      <Checkbox
                        checked={isChecked}
                        onChange={() => toggle(item.id)}
                        label={item.label}
                        onClick={(e) => e.stopPropagation()}
                        style={{
                          fontWeight: "bold",
                          ...(isChecked && {
                            ["--fui-Checkbox__indicator--backgroundColor" as any]: "#000",
                            ["--fui-Checkbox__checkmark--color" as any]: "#fff",
                          }),
                        }}
                      />
                    </div>
                  </AccordionHeader>
                  <AccordionPanel className={styles.panelContent}>
                    {item.details && Object.keys(item.details).length > 0
                      ? Object.entries(item.details)
                        .filter(
                          ([key]) =>
                            key !== "QuestionTitle" && key !== "ModuleName"
                        )
                        .map(([_, value], index) => (
                          <div key={index} style={{ marginBottom: "12px" }}>
                            {value}
                          </div>
                        ))
                      : `Details about ${item.label} will go here.`}
                  </AccordionPanel>
                </AccordionItem>
              );
            })}
          </Accordion>
        </div>
      ) : null}
    </div>
  );
};
