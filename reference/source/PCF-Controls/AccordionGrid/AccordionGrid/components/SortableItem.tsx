import { useSortable } from "@dnd-kit/sortable";
import { SortableItemProps } from "../models/props/SortableItemProps";
import { CSS } from '@dnd-kit/utilities';
import * as React from "react";
import { AccordionHeader, AccordionItem, AccordionPanel, Button, Dialog, DialogTrigger, Label, makeStyles, Tag, tokens, Tooltip, MessageBar } from "@fluentui/react-components";
import { AddCircleRegular, DeleteRegular, InfoRegular, ReOrderDotsVerticalFilled, CheckmarkCircleRegular, DismissRegular } from "@fluentui/react-icons";
import { QuestionTypeStyles } from "../types/QuestionType";
import { ConfirmDialog } from "./ConfirmDialog";
import { DetailsView } from "./DetailView/DetailsView";
import { EntityHelper } from "../utils/EntityHelper";
import { ViewType } from "../types/ViewType";
import { DeleteOrDeactivate } from "./DeleteOrDeactivateQuestions";
import { ProjectDataService } from "../services/ProjectDataService";


const useStyles = makeStyles({
    labelTextLeft: {
        borderRadius: tokens.borderRadiusXLarge,
        paddingTop: tokens.spacingHorizontalXS,
        paddingBottom: tokens.spacingHorizontalXS,
        paddingLeft: tokens.spacingHorizontalS,
        paddingRight: tokens.spacingHorizontalS,
        display: 'inline-block',
        fontWeight: tokens.fontWeightSemibold,
        marginRight: tokens.spacingHorizontalM,
    },
    labelTextRight: {
        borderRadius: tokens.borderRadiusXLarge,
        backgroundColor: tokens.colorNeutralBackground4,
        paddingTop: tokens.spacingHorizontalXS,
        paddingBottom: tokens.spacingHorizontalXS,
        paddingLeft: tokens.spacingHorizontalS,
        paddingRight: tokens.spacingHorizontalS,
        display: 'inline-block',
        fontWeight: tokens.fontWeightSemibold,
        marginRight: tokens.spacingHorizontalM,
        color: tokens.colorNeutralForeground3,
        textTransform: 'uppercase',
    },
    labelInactive: {
        borderRadius: tokens.borderRadiusXLarge,
        backgroundColor: tokens.colorNeutralBackground4,
        paddingTop: tokens.spacingHorizontalXS,
        paddingBottom: tokens.spacingHorizontalXS,
        paddingLeft: tokens.spacingHorizontalS,
        paddingRight: tokens.spacingHorizontalS,
        display: 'inline-block',
        fontWeight: tokens.fontWeightSemibold,
        marginRight: tokens.spacingHorizontalM,
        color: tokens.colorNeutralForeground3,
    },
    leftContainer: {
        flex: 1,
        wordWrap: 'break-word',
        whiteSpace: 'normal',
    },
    rightContainer: {
        display: 'flex',
    },
    tagsContainer: {
        display: 'flex',
        gap: '10px'
    },
    panelContainer: {
        position: 'relative',
    },
    editButtonContainer: {
        position: 'absolute',
        top: '20px',
        right: '20px',
        zIndex: 1,
    }
});

export const SortableItem: React.FC<SortableItemProps> = ({
    row,
    dataService,
    isReadOnly,
    entityName,
    context,
    onOpenAddPanel,
    view,
    isScripter
}) => {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
    } = useSortable({ id: row.id });

    const dndStyle = {
        transform: CSS.Transform.toString(transform),
        transition,
    };

    const styles = useStyles();
    const STATUS_CODE_ACTIVE = 1;
    const STATUS_CODE_INACTIVE = 2;

    const projectId = EntityHelper.getProjectIdFromUrl();
    const ReorderProjectQuestionnaire = async (context: ComponentFramework.Context<any>) => {
        const projectDataService = new ProjectDataService(context, context.webAPI);
        console.log("Projectid for reorder: " + projectId);
        await projectDataService.reorderProjectQuestionnaire(projectId);
    };

    // State for error and success messages
    const [showReactivateError, setShowReactivateError] = React.useState(false);
    const [showDeactivateError, setShowDeactivateError] = React.useState(false);
    const [showReactivateSuccess, setShowReactivateSuccess] = React.useState(false);
    const [showInactivateSuccess, setShowInactivateSuccess] = React.useState(false);
    const [isDialogOpen, setIsDialogOpen] = React.useState(false);
    const [isDeactivateMessage, setIsDeactivateMessage] = React.useState<boolean | null>(null);

    const ReactivateDialogTitle = "Confirm Reactivation";
    const ReactivateDialogText = "Do you want to reactivate the selected Questionnaire Line? This action will change the status of the selected Questionnaire Line to Active.";
    const ReactivatePrimaryButtonText = "Reactivate";
    const ReactivateSecondaryButtonText = "Cancel";

    function handleInactivateClick(e: React.MouseEvent<HTMLButtonElement>) {
        e.stopPropagation();
    }


    function handleReactivateClick(e: React.MouseEvent<HTMLButtonElement>) {
        e.stopPropagation();
    }

    async function handleReactivateConfirmClick(id: string) {
        const result = await dataService.reactivateRecord(entityName, id);

        if (result.success) {
            ReorderProjectQuestionnaire(context);
            context.parameters.gridDataSet.refresh();
            // Show success message popup
            setShowReactivateSuccess(true);
            // Auto-hide after 5 seconds
            setTimeout(() => setShowReactivateSuccess(false), 5000);
        } else {
            // Show error message popup
            setShowReactivateError(true);
            // Auto-hide after 5 seconds
            setTimeout(() => setShowReactivateError(false), 5000);
        }
    }

    return (
        <div style={dndStyle}>
            {/* Error message popup for reactivation failure */}
            {showReactivateError && (
                <MessageBar
                    intent="error"
                    style={{
                        position: 'fixed',
                        top: '20px',
                        right: '20px',
                        zIndex: 9999,
                        maxWidth: '400px'
                    }}
                >
                    Failed to reactivate the record. Please try again.
                    <Button
                        appearance="transparent"
                        icon={<DismissRegular />}
                        onClick={() => setShowReactivateError(false)}
                        style={{ marginLeft: 'auto' }}
                    />
                </MessageBar>
            )}

            {/* Error message popup for deactivation failure */}
            {showDeactivateError && (
                <MessageBar
                    intent="error"
                    style={{
                        position: 'fixed',
                        top: '20px',
                        right: '20px',
                        zIndex: 9999,
                        maxWidth: '400px'
                    }}
                >  {isDeactivateMessage ? " Failed to deactivate the record. Please try again." : " Failed to delete the record. Please try again."}

                    <Button
                        appearance="transparent"
                        icon={<DismissRegular />}
                        onClick={() => setShowDeactivateError(false)}
                        style={{ marginLeft: 'auto' }}
                    />
                </MessageBar>
            )}

            {/* Success message popup for reactivation */}
            {showReactivateSuccess && (
                <MessageBar
                    intent="success"
                    style={{
                        position: 'fixed',
                        top: '20px',
                        right: '20px',
                        zIndex: 9999,
                        maxWidth: '400px'
                    }}
                >
                    Record reactivated successfully.
                    <Button
                        appearance="transparent"
                        icon={<DismissRegular />}
                        onClick={() => setShowReactivateSuccess(false)}
                        style={{ marginLeft: 'auto' }}
                    />
                </MessageBar>
            )}

            {/* Success message popup for deactivation */}
            {showInactivateSuccess && (
                <MessageBar
                    intent="success"
                    style={{
                        position: 'fixed',
                        top: '20px',
                        right: '20px',
                        zIndex: 9999,
                        maxWidth: '400px'
                    }}
                > {isDeactivateMessage ? "Record deactivated successfully." : "Record deleted successfully."}

                    <Button
                        appearance="transparent"
                        icon={<DismissRegular />}
                        onClick={() => setShowInactivateSuccess(false)}
                        style={{ marginLeft: 'auto' }}
                    />
                </MessageBar>
            )}

            <AccordionItem key={row.id} value={row.id}>
                <AccordionHeader>
                    <div className={styles.leftContainer}>
                        <Label
                            className={row.statusCode === STATUS_CODE_INACTIVE ? styles.labelInactive : styles.labelTextLeft}
                            style={row.statusCode !== STATUS_CODE_INACTIVE ? QuestionTypeStyles[row.firstLabelId] : undefined}
                        >
                            {row.firstLabelText}
                        </Label>
                        <span className={row.statusCode === STATUS_CODE_INACTIVE ? 'strikethrough' : ''}>
                            {row.name}
                        </span>
                    </div>
                    <div className={styles.rightContainer}>
                        <div className={styles.tagsContainer}>
                            {row.middleLabelText ? (
                                <Tag appearance='outline'>
                                    {row.middleLabelText}
                                </Tag>
                            ) : null}

                            <Tag>
                                {row.lastLabelText}
                            </Tag>
                        </div>

                        <Tooltip
                            content={
                                <div>
                                    <div><strong>Question Version:</strong> {row.questionVersion || 'N/A'}</div>
                                    <div><strong>Question Rationale:</strong> {row.questionRationale || 'N/A'}</div>
                                </div>
                            }
                            relationship="label"
                        >
                            <Button
                                appearance="transparent"
                                icon={<InfoRegular />}
                                onClick={(e) => {
                                    e.stopPropagation();
                                    console.log("Info button clicked for row:", row.id);
                                }}
                            >
                            </Button>
                        </Tooltip>

                        {(!isReadOnly || (isScripter && row.isDummy === "True")) ? (
                            <>
                                {/* Deactivate button - shown only for active rows */}
                                {row.statusCode === STATUS_CODE_ACTIVE && (
                                    <Dialog open={isDialogOpen} onOpenChange={(_, data) => setIsDialogOpen(data.open)}>
                                        <DialogTrigger disableButtonEnhancement>
                                            <Button
                                                appearance="transparent"
                                                icon={<DeleteRegular />}
                                                onClick={handleInactivateClick}
                                                data-testid="delete-button"
                                            >
                                            </Button>
                                        </DialogTrigger>
                                        <DeleteOrDeactivate
                                            context={context}
                                            questionId={row.id}
                                            onSuccess={(isDeactivate) => {
                                                setIsDeactivateMessage(isDeactivate);

                                                setShowInactivateSuccess(true);
                                                setTimeout(() => {
                                                    setShowInactivateSuccess(false);
                                                    setIsDialogOpen(false);
                                                }, 5000);
                                            }}
                                            onError={(isDeactivate) => {
                                                setIsDeactivateMessage(isDeactivate);
                                                setShowDeactivateError(true);
                                                setTimeout(() => {
                                                    setShowDeactivateError(false);
                                                    setIsDialogOpen(false);
                                                }, 5000);
                                            }}
                                        />
                                    </Dialog>
                                )}

                                {/* Reactivate button - shown only for inactive rows */}
                                {row.statusCode === STATUS_CODE_INACTIVE && (
                                    <Dialog>
                                        <DialogTrigger disableButtonEnhancement>
                                            <Button
                                                appearance="transparent"
                                                icon={<CheckmarkCircleRegular />}
                                                onClick={handleReactivateClick}
                                                data-testid="reactivate-button"
                                            >
                                            </Button>
                                        </DialogTrigger>
                                        <ConfirmDialog
                                            context={context}
                                            dialogTitle={ReactivateDialogTitle}
                                            dialogText={ReactivateDialogText}
                                            buttonPrimaryText={ReactivatePrimaryButtonText}
                                            buttonSecondaryText={ReactivateSecondaryButtonText}
                                            onPrimaryActionClick={() => handleReactivateConfirmClick(row.id)}
                                            onSecondaryActionClick={(e) => {
                                                e.stopPropagation();
                                            }}
                                        />
                                    </Dialog>
                                )}

                                {/* Add questions button - disabled for inactive rows */}
                                <Button
                                    appearance="transparent"
                                    icon={<AddCircleRegular />}
                                    onClick={(e) => {
                                        e.stopPropagation();
                                        onOpenAddPanel(row);
                                        console.log("Button clicked without toggling accordion");
                                    }}
                                    disabled={row.statusCode === STATUS_CODE_INACTIVE}
                                >
                                </Button>
                            </>
                        ) : null}
                        {/* Drag&Drop handle button */}
                        {!isReadOnly && view === ViewType.Active ? (
                            <Button
                                ref={setNodeRef}
                                style={{ cursor: "grab", marginLeft: 8 }}
                                aria-label="Drag to reorder"
                                appearance="transparent"
                                icon={<ReOrderDotsVerticalFilled />}
                                {...listeners}
                                {...attributes}
                                onClick={(e) => {
                                    e.stopPropagation();
                                }}
                            >
                            </Button>
                            ) : null
                        }
                    </div>
                </AccordionHeader>
                <AccordionPanel>
                    <div className={styles.panelContainer}>
                        {(!isReadOnly || isScripter) && row.statusCode === STATUS_CODE_ACTIVE ? (
                            <div className={styles.editButtonContainer}>
                                {/* Edit Button for redirecting to Form in MDA */}
                                <Button
                                    appearance="primary"
                                    size="small"
                                    data-testid="edit-button"
                                    onClick={async (e) => {
                                        e.stopPropagation();
                                        console.log("Edit button clicked for record ID:", row.id);
                                        // Generate dynamic MDA URL using org URL from context
                                        const dynamicUrl = await EntityHelper.generateEditUrl(context, entityName, row.id);
                                        console.log("Opening Edit URL:", dynamicUrl);
                                        // Open the form in the same tab
                                        window.location.href = dynamicUrl;
                                    }}
                                >
                                    Edit
                                </Button>
                            </div>
                        ) : null}
                        <DetailsView
                            row={row}
                            context={context}
                        />
                    </div>
                </AccordionPanel>
            </AccordionItem>
        </div>
    );
};