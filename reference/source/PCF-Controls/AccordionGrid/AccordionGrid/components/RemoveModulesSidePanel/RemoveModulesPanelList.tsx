/**
 * RemoveModulesPanelList.tsx
 *
 * This component renders list of modules, and their questions available in project to deactivate
 */
import * as React from "react";
import type { RowEntity } from "../../models/RowEntity";
import type { ModulesInProject } from "../../models/ModulesInProject";
import {
    Accordion,
    AccordionItem,
    AccordionHeader,
    AccordionPanel,
    Checkbox,
    makeStyles,
    shorthands,
    Label,
    tokens
} from "@fluentui/react-components";
import { QuestionTypeStyles } from "../../types/QuestionType";

interface RemoveModulesPanelListProps {
    modulesInProject: ModulesInProject[];
    selectedIds: string[];
    onSelectionChange?: (ids: string[]) => void;
}

const useStyles = makeStyles({
    accordionWrapper: {
        backgroundColor: '#ffffff',
        borderRadius: '4px',
        padding: '12px',
        maxHeight: 'auto',
    },
    moduleHeader: {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        width: '100%',
        ...shorthands.padding("4px", "0"),
    },
    moduleContainer: {
        border: `1px solid ${tokens.colorNeutralStroke1}`, // thin gray border
        borderRadius: tokens.borderRadiusMedium,
        marginBottom: tokens.spacingVerticalS,
        overflow: 'hidden',
        transition: 'all 0.3s ease', // smooth expand/collapse effect
        backgroundColor: tokens.colorNeutralBackground1,
    },
    moduleName: {
        fontWeight: tokens.fontWeightSemibold,
    },
    selectAllText: {
        fontWeight: tokens.fontWeightSemibold,
    },
    questionList: {
        marginLeft: '24px',
        listStyleType: 'none',
        padding: 0,
        transition: 'max-height 0.3s ease, padding 0.3s ease', // smooth animation
    },
    labelTextLeft: {
        borderRadius: tokens.borderRadiusXLarge,
        paddingTop: tokens.spacingHorizontalXS,
        paddingBottom: tokens.spacingHorizontalXS,
        paddingLeft: tokens.spacingHorizontalS,
        paddingRight: tokens.spacingHorizontalS,
        display: "inline-block",
        fontWeight: tokens.fontWeightSemibold,
        marginRight: tokens.spacingHorizontalM,
    },
});

export const RemoveModulesPanelList: React.FC<RemoveModulesPanelListProps> = ({
    modulesInProject,
    selectedIds,
    onSelectionChange
}) => {
    const styles = useStyles();

    if (modulesInProject.length === 0) {
        return <p>No modules available to remove.</p>;
    }

    // Flatten all row IDs across modules - For select all
    const allRowIds = modulesInProject.flatMap(m => m.rows.map(r => r.id));
    const allSelected = allRowIds.length > 0 && allRowIds.every(id => selectedIds.includes(id));

    const toggleAllModules = () => {
        if (allSelected) {
            // unselect everything
            onSelectionChange?.([]);
        } else {
            // select everything
            onSelectionChange?.(allRowIds);
        }
    };

    const toggleModule = (rows: RowEntity[]) => {
        const rowIds = rows.map(r => r.id);
        const isSelected = rowIds.every(id => selectedIds.includes(id));

        let next: string[];
        if (isSelected) {
            // unselect all rows from this module
            next = selectedIds.filter(id => !rowIds.includes(id));
        } else {
            // add all rows from this module (avoid duplicates)
            next = Array.from(new Set([...selectedIds, ...rowIds]));
        }

        onSelectionChange?.(next);
    };

    return (
        <div className={styles.accordionWrapper}>
            {/* Select All Checkbox */}
            <div style={{ marginBottom: "12px", display: "flex", alignItems: "center", gap: "8px" }}>
                <Checkbox
                    checked={allSelected}
                    onChange={toggleAllModules}
                    aria-label="Select All Modules"
                    style={{
                        fontWeight: "bold",
                        ...(allSelected && {
                            ["--fui-Checkbox__indicator--backgroundColor" as any]: "#000",
                            ["--fui-Checkbox__checkmark--color" as any]: "#fff",
                        }),
                    }}
                />
                <span className={styles.selectAllText}>Select All ({modulesInProject.length} modules)</span>
            </div>


            <Accordion multiple collapsible>
                {modulesInProject.map(module => {
                    const rowIds = module.rows.map(r => r.id);
                    const isChecked = rowIds.every(id => selectedIds.includes(id));

                    return (
                        <AccordionItem key={module.moduleName} value={module.moduleName} className={styles.moduleContainer}>
                            <AccordionHeader className={styles.moduleHeader}>
                                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', width: '100%' }}>
                                    {/* Left: checkbox + module name */}
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                        <Checkbox
                                            checked={isChecked}
                                            onChange={() => toggleModule(module.rows)}
                                            aria-label={`Select ${module.moduleName}`}
                                            onClick={(e) => e.stopPropagation()}
                                            style={{
                                                fontWeight: "bold",
                                                ...(isChecked && {
                                                    ["--fui-Checkbox__indicator--backgroundColor" as any]: "#000",
                                                    ["--fui-Checkbox__checkmark--color" as any]: "#fff",
                                                }),
                                            }}
                                        />
                                        <span className={styles.moduleName}>{module.moduleName}</span>
                                    </div>

                                    {/* Right: question count */}
                                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                                        <span>{module.count} questions</span>
                                    </div>
                                </div>
                            </AccordionHeader>

                            <AccordionPanel className={styles.questionList}>
                                <ul>
                                    {module.rows.map((row: RowEntity) => (
                                        <li
                                            key={row.id}
                                            style={{
                                                display: "flex",
                                                alignItems: "flex-start",
                                                gap: tokens.spacingHorizontalS,
                                                marginBottom: tokens.spacingVerticalS,
                                            }}
                                        >
                                            <div style={{ flex: '0 0 auto' }}>
                                                <Label
                                                    className={styles.labelTextLeft}
                                                    style={{ ...QuestionTypeStyles[row.firstLabelId], whiteSpace: 'nowrap' }}
                                                >
                                                    {row.firstLabelText}
                                                </Label>
                                            </div>

                                            <div style={{ flex: 1 }}>
                                                <span style={{ display: 'inline-block' }}>
                                                    {row.questionTitle}
                                                </span>
                                            </div>
                                        </li>
                                    ))}
                                </ul>
                            </AccordionPanel>
                        </AccordionItem>

                    );
                })}
            </Accordion>
        </div>
    );
};
