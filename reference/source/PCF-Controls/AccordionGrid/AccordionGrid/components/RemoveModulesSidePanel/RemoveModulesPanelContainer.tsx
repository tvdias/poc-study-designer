/**
 * RemoveModulesPanelContainer.tsx
 *
 * This component renders a Side Panel container for removing modules.
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
    Dialog,
    DialogSurface,
    DialogBody,
    DialogContent,
    DialogActions,
    Spinner
} from "@fluentui/react-components";
import { Dismiss20Regular } from "@fluentui/react-icons";
import { PanelActionButtons } from "./RemoveModulesPanelActionButtons";
import type { RowEntity } from "../../models/RowEntity";
import { DataService } from "../../services/DataService";
import { RemoveModulesPanelList } from "./RemoveModulesPanelList";
import { GirdRowsHelper } from "./../../utils/GridRowsHelper"
import { ProjectDataService } from "../../services/ProjectDataService";
import { EntityHelper } from "../../utils/EntityHelper";

const usePanelStyles = makeStyles({
    drawerBody: {
        backgroundColor: "#F5F5F4",
        height: "100%",
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

export interface RemoveModulesPanelContainerProps {
    isOpen: boolean;
    onClose: () => void;
    rows: RowEntity[];
    context: ComponentFramework.Context<any>;
    isReadOnly: boolean;
    entityName: string;
}

export const RemoveModulesPanelContainer: React.FC<RemoveModulesPanelContainerProps> = ({
    isOpen,
    onClose,
    rows,
    context,
    isReadOnly,
    entityName
}) => {
    const headerStyles = useHeaderStyles();
    const panelStyles = usePanelStyles();

    const gridHelper = new GirdRowsHelper();
    const modulesInProject = gridHelper.getModulesInProject(rows);

    const [selectedIds, setSelectedIds] = React.useState<string[]>([]);
    const [dialogOpen, setDialogOpen] = React.useState(false);
    const [dialogMessage, setDialogMessage] = React.useState("");
    const [loading, setLoading] = React.useState(false);
    const [confirmCloseDialog, setConfirmCloseDialog] = React.useState(false);
    
    const projectId = EntityHelper.getProjectId(context);
    
    const ConfirmCloseDialogText = "Any selections made will be removed. Are you sure you want to close?";

    const handleRemove = async (e: React.MouseEvent<HTMLButtonElement>) => {
        if (selectedIds.length === 0) return;

        try {
            setLoading(true);
            const dataService = new DataService(context.webAPI);

            const results = await Promise.allSettled(
                selectedIds.map(id => dataService.inactivateRecord(entityName, id))
            );

            // Count the number of modules affected
            const affectedModules = modulesInProject.filter(module =>
                module.rows.some(row => selectedIds.includes(row.id))
            ).length;

            const failed = results.filter(r => r.status === "rejected");
            const succeededCount = affectedModules - failed.length;

            if (failed.length > 0) {
                setDialogMessage(`${succeededCount} module${succeededCount > 1 ? 's' : ''} deactivated, but some failed.`);
            } else {
                setDialogMessage(`${succeededCount} module${succeededCount > 1 ? 's' : ''} successfully deactivated!`);
            }

            setDialogOpen(true);

            const projectDataService = new ProjectDataService(context, context.webAPI);
            console.log("Projectid for reorder: " + projectId);
            await projectDataService.reorderProjectQuestionnaire(projectId);
        
        } catch (err) {
            console.error("Error removing modules:", err);
            setDialogMessage("Error removing modules. Check console for details.");
            setDialogOpen(true);
        }
        finally {
            setLoading(false);
        }
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

    const handleDialogClose = () => {
        setDialogOpen(false);
        onClose();
        context.parameters.gridDataSet.refresh();
    };

    // Reset selections on close / re-open of panel
    React.useEffect(() => {
        if (!isOpen) {
            setSelectedIds([]);
        }
    }, [isOpen]);

    return (
        <>
            <Drawer
                open={isOpen}
                onOpenChange={(_, data) => {
                    // prevent immediate close
                    if (selectedIds.length > 0) {
                        setConfirmCloseDialog(true); // show confirmation
                    } else {
                        onClose(); // safe to close
                    }
                }}
                position="end"
                size="medium"
                className={panelStyles.drawer}
                modalType="modal"
            >
                <DrawerHeader>
                    <div className={headerStyles.headerRow}>
                        <DrawerHeaderTitle>Remove Modules</DrawerHeaderTitle>
                        <Button
                            appearance="transparent"
                            onClick={attemptClose}
                            aria-label="Close"
                            style={{ padding: 0 }}
                        >
                            <Dismiss20Regular style={{ width: "28px", height: "28px" }} />
                        </Button>
                    </div>
                    <p style={{ marginTop: "4px", color: "#666" }}>
                        Select the modules you want to remove from this questionnaire.
                    </p>
                </DrawerHeader>

                <DrawerBody>
                    <RemoveModulesPanelList
                        modulesInProject={modulesInProject}
                        selectedIds={selectedIds}
                        onSelectionChange={setSelectedIds}
                    />
                </DrawerBody>

                <DrawerFooter>
                    <PanelActionButtons
                        onCancel={attemptClose}
                        handleRemove={handleRemove}
                        context={context}
                        disabled={isReadOnly || selectedIds.length === 0}
                    />
                </DrawerFooter>

                {loading && (
                    <div
                        style={{
                            position: "fixed",
                            top: 0,
                            left: 0,
                            right: 0,
                            bottom: 0,
                            backgroundColor: "rgba(0,0,0,0.5)",
                            display: "flex",
                            alignItems: "center",
                            justifyContent: "center",
                            zIndex: 2147483647, // MAX safe int for CSS z-index 
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
            </Drawer>

            {/* Standard success/error dialog */}
            <Dialog open={dialogOpen} onOpenChange={(_, data) => setDialogOpen(data.open)}>
                <DialogSurface>
                    <DialogBody>
                        <p>{dialogMessage}</p>
                        <DialogActions>
                            <Button appearance="primary" onClick={handleDialogClose}>
                                OK
                            </Button>
                        </DialogActions>
                    </DialogBody>
                </DialogSurface>
            </Dialog>

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
        </>
    );
};
