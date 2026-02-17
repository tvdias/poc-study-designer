/**
 * AddQuestionsPanelContainer.tsx
 * 
 * This component renders the Side panel container, which calls the sub-components.
 */
import * as React from "react";
import {
    Drawer,
    DrawerBody,
    DrawerHeader,
    DrawerHeaderTitle,
    DrawerFooter,
    Button,
    makeStyles,
    Spinner,
    Dialog,
    DialogSurface,
    DialogBody,
    DialogContent,
    DialogActions
} from "@fluentui/react-components";

import { Dismiss20Regular } from "@fluentui/react-icons";
import { AddQuestionsPanelContainerProps } from "../../models/props/AddQuestionsPanelContainerProps";
import { PanelActionButtons } from "./AddQuestionsPanelActionButtons";
import { PanelSearch } from "./AddQuestionsPanelSearch";
import { PanelTabFilters } from "./AddQuestionsPanelTabFilters";
import { QuestionDataService } from "../../services/QuestionDataService";
import { ModuleDataService } from "../../services/ModuleDataService";
import type { QuestionEntity } from "../../models/QuestionEntity";
import type { ModuleEntity } from "../../models/ModuleEntity";
import type { SearchType } from "../../types/AddQuestionsSidePanel/SearchType";
import { stripHtml } from "../../utils/StringHelper";
import { EntityHelper } from "../../utils/EntityHelper";
import { ProjectDataService } from "../../services/ProjectDataService";
import { ConfirmDialog } from "../../components/ConfirmDialog";

const usePanelStyles = makeStyles({
    drawerBody: {
        backgroundColor: "#F5F5F4",
        height: "100%", // optional, ensures full panel coverage
    },
    drawer: {
        backgroundColor: "#F5F5F4",
    },
});

const useHeaderStyles = makeStyles({
    headerRow: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        width: "100%",
    },
});

export const AddQuestionsPanelContainer: React.FC<AddQuestionsPanelContainerProps> = ({
    isOpen,
    onClose,
    row,
    existingRows,
    onRefresh,
    addFromHeader,
    context,
    isScripter
}) => {
    const headerStyles = useHeaderStyles();
    const panelStyles = usePanelStyles();

    const [allQuestions, setAllQuestions] = React.useState<QuestionEntity[]>([]);
    const [allModules, setAllModules] = React.useState<ModuleEntity[]>([]);
    const [visibleResults, setVisibleResults] = React.useState<SearchType[]>([]);
    const [selectedIds, setSelectedIds] = React.useState<string[]>([]);
    const [selectedTab, setSelectedTab] = React.useState<"questions" | "modules" | "custom">("questions");
    const [loading, setLoading] = React.useState(false);
    const [notification, setNotification] = React.useState<string | null>(null);

    // Dialog box after API is complete
    const [dialogOpen, setDialogOpen] = React.useState(false);
    const [dialogMessage, setDialogMessage] = React.useState<string | null>(null);

    // Dialog box to confirm closure of panel
    const [confirmCloseDialog, setConfirmCloseDialog] = React.useState(false);
    const ConfirmCloseDialogText = "Any selections made will be removed. Are you sure you want to close?";
    let projectId: string = "";

    // Fetch data when panel opens
    React.useEffect(() => {
        if (!isOpen) return;

        const webApi = (window as any)?.Xrm?.WebApi;
        if (!webApi) return; // stop if WebApi not ready

        const safeExistingRows = existingRows || [];
        const questionDataService = new QuestionDataService(webApi);
        const moduleDataService = new ModuleDataService(webApi);

        // Get projectId from the correct source
        const projectId = addFromHeader ? EntityHelper.getProjectIdFromUrl() : row?.projectId;

        (async () => {
            try {
                let standardOrCustom: 0 | 1 | undefined;
                if (selectedTab === "questions") standardOrCustom = 0;
                else if (selectedTab === "custom") standardOrCustom = 1;

                const { questions } = await questionDataService.getActiveQuestions(safeExistingRows,isScripter,  standardOrCustom, projectId);
                setAllQuestions(questions || []);
            } catch (err) {
                console.error("Error fetching questions", err);
                setAllQuestions([]);
            }

            try {
                // Get all questionnaire lines in this project
                const questionnaireLines = await questionDataService.GetQuestionsModulesByProject(projectId);

                // Extract module IDs already linked in the project
                const moduleIds = questionnaireLines
                    .map((q: any) => q["_ktr_module_value"])
                    .filter(Boolean);

                // Get module details for those IDs
                const modulesInProject = await moduleDataService.getModulesByIds(moduleIds);

                // Get all active modules
                const activeModules = await moduleDataService.getActiveModules();

                // Filter out modules already used in the project
                const existingNames = new Set(
                    modulesInProject.map((m: any) => m.moduleName.toLowerCase())
                );

                const availableModules = activeModules.filter(
                    (m: any) => !existingNames.has(m.moduleName.toLowerCase())
                );

                setAllModules(availableModules || []);
            } catch (err) {
                console.error("Error fetching modules", err);
                setAllModules([]);
            }
        })();
    }, [isOpen, existingRows, selectedTab]);

    // Reset search & selection when tab changes
    React.useEffect(() => {
        setVisibleResults([]);
        setSelectedIds([]);
    }, [selectedTab]);

    // Map current tab items to unified SearchType[]
    const currentItems: SearchType[] = React.useMemo(() => {
        if (selectedTab === "modules") {
            return allModules.map(m => ({
                id: m.id || "temp-id",
                label: m.moduleName || "Untitled",
                type: "module",
                details: {
                    ModuleName: m.moduleName || "",
                    ModuleLabel: m.moduleLabel || "",
                    ModuleDescription: m.moduleDescription || "",
                },
            }));
        } else {
            // questions or custom
            return allQuestions.map(q => ({
                id: q.id || "temp-id",
                label: q.questionTitle || q.questionVariableName || "Untitled",
                type: "question",
                details: {
                    QuestionTitle: q.questionTitle || "",
                    QuestionVariableName: q.questionVariableName || "",
                    QuestionType: q.questionType || "",
                    QuestionText: stripHtml(q.questionText || ""),
                },
                hiddenSearchFields: [q.questionVariableName || ""],
            }));
        }
    }, [selectedTab, allQuestions, allModules]);

    // Reset tab + clear selections + clear results when panel opens
    React.useEffect(() => {
        if (isOpen) {
            setSelectedTab("questions");
            setSelectedIds([]);
            setVisibleResults([]);
        }
    }, [isOpen]);

    // On Save send API request body and call API
    const handleSave = async () => {

        let apiErrorMessage: string | null = null;

        let requestBody: {
            projectId: string;
            sortOrder: number;
            entityType: string;
            rows: any;
        };

        if (selectedIds.length === 0) {
            setDialogMessage("Please select at least one Question or Module to add to the Project.");
            setDialogOpen(true);
            return;
        }

        if (addFromHeader) {
            projectId = EntityHelper.getProjectIdFromUrl();
            if (!projectId) {
                setDialogMessage("Could not find project ID in the URL.");
                setDialogOpen(true);
                return;
            }

            requestBody = {
                projectId: projectId,
                sortOrder: -1,
                entityType: selectedTab === "modules" ? "Module" : "Question",
                rows: selectedIds.map((id) => ({ Id: id })),
            };
        } else {
            if (!row) return;
            
            projectId = row.projectId;

            requestBody = {
                projectId: projectId,
                sortOrder: (row.sortOrder || 0) + 1,
                entityType: selectedTab === "modules" ? "Module" : "Question",
                rows: selectedIds.map((id) => ({ Id: id })),
            };
        }

        try {
            setLoading(true);
            setDialogMessage(null);

            const webApi = (window as any)?.Xrm?.WebApi;
            if (!webApi) throw new Error("Xrm.WebApi not available");

            const request = {
                getMetadata: () => ({
                    boundParameter: null,
                    parameterTypes: {
                        projectId: { typeName: "Edm.String", structuralProperty: 1 },
                        sortOrder: { typeName: "Edm.Int32", structuralProperty: 1 },
                        entityType: { typeName: "Edm.String", structuralProperty: 1 },
                        rows: { typeName: "Edm.String", structuralProperty: 1 },
                    },
                    operationType: 0,
                    operationName: "ktr_add_questions_or_modules_unbound",
                }),
                projectId: requestBody.projectId,
                sortOrder: requestBody.sortOrder,
                entityType: requestBody.entityType,
                rows: JSON.stringify(requestBody.rows),
            };

            const response = await webApi.online.execute(request);

            if (response.ok) 
            {
                if (selectedTab === "modules") {
                    setDialogMessage("Selected Modules have been added to the project successfully.");
                } else if (selectedTab === "custom") {
                    setDialogMessage("Custom Questions have been added to the project successfully.");
                } else {
                    setDialogMessage("Selected Questions have been added to the project successfully.");
                }
            } else {
                apiErrorMessage = "Failed to add records. Please try again.";
            }
        } catch (err: any) {
            console.error("Error calling custom API", err);

            apiErrorMessage = "An error occurred while adding records.";

            // If API returned a detailed error message
            if (err && err.message) {
                apiErrorMessage = err.message;
            }

            // If it's an OData error from WebApi
            if (err && err.error && err.error.message) {
                apiErrorMessage = err.error.message;
            }

        } finally {
             try {
                // Call reorder regardless of API success/failure
                await ReorderProjectQuestionnaire(context);
            } catch (reorderErr) {
                console.error("Error reordering project questionnaire:", reorderErr);
            }

            setLoading(false);
            setDialogOpen(true);

            // show API error if present
            if (apiErrorMessage) {
                setDialogMessage(apiErrorMessage);
            }
        }
    };

    const ReorderProjectQuestionnaire = async (context: ComponentFramework.Context<any>) => {
        const projectDataService = new ProjectDataService(context, context.webAPI);
        console.log("Projectid for reorder: " + projectId);
        await projectDataService.reorderProjectQuestionnaire(projectId);
    };

    const handleDialogClose = () => {
        setDialogOpen(false);
        onClose();    // close side panel
        onRefresh();  // refresh parent PCF
    };

    const attemptClose = () => {
        if (selectedIds.length > 0) {
            setConfirmCloseDialog(true);
        } else {
            onClose();
        }
    };
    const handleConfirmClose = () => {
        setConfirmCloseDialog(false);
        onClose();
    };
    const handleCancelClose = () => {
        setConfirmCloseDialog(false);
    };

    return (
        <Drawer open={isOpen} onOpenChange={(_, data) => {
            // prevent immediate close
            if (selectedIds.length > 0) {
                setConfirmCloseDialog(true); // show confirmation
            } else {
                onClose(); // safe to close
            }
        }} position="end" size="medium" className={panelStyles.drawer} modalType="modal">
            <DrawerHeader>
                <div className={headerStyles.headerRow}>
                    <DrawerHeaderTitle>Import from Library</DrawerHeaderTitle>
                    <Button appearance="transparent" onClick={attemptClose} aria-label="Close" style={{ padding: 0 }}>
                        <Dismiss20Regular style={{ width: "28px", height: "28px" }} />
                    </Button>
                </div>
                <p style={{ marginTop: "4px", color: "#666" }}>Select records</p>
            </DrawerHeader>

            <DrawerBody>
                <PanelTabFilters
                    selectedTab={selectedTab}
                    onTabChange={tab => setSelectedTab(tab as "questions" | "modules" | "custom")}
                    isScripter={isScripter}
                />

                <div style={{ marginTop: "20px" }}>
                    <PanelSearch
                        items={currentItems}
                        onResults={setVisibleResults}
                        selectedIds={selectedIds}
                        onSelectionChange={setSelectedIds}
                       
                    />
                </div>
            </DrawerBody>

            {loading && (
                <div
                    style={{
                        position: "fixed",   // cover the entire viewport
                        top: 0,
                        left: 0,
                        right: 0,
                        bottom: 0,
                        backgroundColor: "rgba(0,0,0,0.5)", // semi-transparent dark bg
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        zIndex: 2000, // make sure it's above the Drawer
                    }}
                >
                    <div
                        style={{
                            background: "white",
                            padding: "20px 30px",
                            borderRadius: "8px",
                            boxShadow: "0 4px 12px rgba(0,0,0,0.15)",
                            display: "flex",
                            flexDirection: "column",
                            alignItems: "center",
                            gap: "12px",
                        }}
                    >
                        <Spinner size="large" />
                        <span style={{ fontSize: "16px", fontWeight: 500, color: "#333" }}>
                            Processing...
                        </span>
                    </div>
                </div>
            )}

            {/* Standard success/error dialog */}
            {dialogOpen && (
                <Dialog open={dialogOpen}>
                    <ConfirmDialog
                        context={context}
                        dialogTitle="" // No title, just success/error message
                        dialogText={dialogMessage || ""}
                        buttonPrimaryText="OK"
                        buttonSecondaryText=""  // No secondary button needed
                        onPrimaryActionClick={handleDialogClose}
                        onSecondaryActionClick={() => {}} // No-op
                    />
                </Dialog>
            )}

            {/* Confirmation dialog on close button, cancel button, click outside panel */}
            <Dialog open={confirmCloseDialog} onOpenChange={(_, data) => setConfirmCloseDialog(data.open)}>
                <DialogSurface
                >
                    <DialogBody>
                        <DialogContent>
                            {ConfirmCloseDialogText}
                        </DialogContent>
                        <DialogActions>
                            <Button appearance="secondary" onClick={handleCancelClose}>
                                Cancel
                            </Button>
                            <Button appearance="primary" onClick={handleConfirmClose}>
                                Yes, Close
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>


            <DrawerFooter>
                <PanelActionButtons onCancel={onClose} onSave={handleSave} />
            </DrawerFooter>
        </Drawer>
    );
};
