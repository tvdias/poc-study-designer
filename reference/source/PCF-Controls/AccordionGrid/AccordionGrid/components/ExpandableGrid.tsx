import * as React from 'react';
import {
  DndContext, 
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from '@dnd-kit/core';
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { ExpandableGridProps } from '../models/props/ExpandableGridProps';
import { useState, useMemo } from 'react';
import { SortableItem } from './SortableItem';
import { AddQuestionsPanelContainerProps } from '../models/props/AddQuestionsPanelContainerProps';
import { AddQuestionsPanelContainer } from './AddQuestionsSidePanel/AddQuestionsPanelContainer';
import { RowEntity } from '../models/RowEntity';
import { Accordion, Spinner } from '@fluentui/react-components';
import { debounce } from 'throttle-debounce';

export const ExpandableGrid: React.FC<ExpandableGridProps> = ({
    context,
    rows,
    dataService,
    entityName,
    isReadOnly,
    view,
    isScripter
}) => {
    const [items, setItems] = useState(rows);
    const [isSavingOrder, setIsSavingOrder] = React.useState(false);

    // Sync local state with latest rows
    React.useEffect(() => {
        setItems(rows);
    }, [rows]);
    
    const sensors = useSensors(
        useSensor(PointerSensor),
        useSensor(KeyboardSensor, {
        coordinateGetter: sortableKeyboardCoordinates,
        })
    );

    //Constants for state of Side Panel
    const [isPanelOpen, setIsPanelOpen] = React.useState(false);
    const [selectedRow, setSelectedRow] = React.useState<AddQuestionsPanelContainerProps["row"]>();
    
    const debouncedSaveSortOrder = React.useMemo(() => {
        return debounce(2000, async (newRows: RowEntity[]) => {
            try {
                setIsSavingOrder(true);
                await dataService.saveOrder("kt_questionnairelines", "kt_questionsortorder", newRows);
                context.parameters.gridDataSet.refresh();
            } finally {
                setIsSavingOrder(false);
            }
        });
    }, [dataService, context.parameters.gridDataSet]);

    function handleDragStart() {
        // cancel any pending save when user starts dragging again
        debouncedSaveSortOrder.cancel({ upcomingOnly: true });
        setIsSavingOrder(false);
    }

    function handleDragEnd(event: DragEndEvent) {
        const {active, over} = event;
        
        if (!over) return;

        if (active.id !== over.id) {
            setItems((items) => {
                const oldIndex = items.findIndex((row) => row.id === active.id);
                const newIndex = items.findIndex((row) => row.id === over.id);
                
                const newRows = arrayMove(items, oldIndex, newIndex)
                    .map((row, index) => ({ ...row, sortOrder: index }));

                // save changes
                debouncedSaveSortOrder(newRows);

                return newRows;
            });
        }
    }

    function handleOpenAddPanel(row: RowEntity) {
        setSelectedRow(row);
        setIsPanelOpen(true);
    }

    function refreshData() {
        context.parameters.gridDataSet.refresh();
    }

    return (
        <>
        <div style={{position: "relative"}}>
            <DndContext 
                sensors={sensors}
                collisionDetection={closestCenter}
                onDragStart={handleDragStart}
                onDragEnd={handleDragEnd}
            >
            <Accordion collapsible>
                <SortableContext 
                    items={items}
                    strategy={verticalListSortingStrategy}
                >
                    {items.map(item => <SortableItem 
                        key={item.id}
                        row={item}
                        dataService={dataService}
                        isReadOnly={isReadOnly}
                        entityName={entityName}
                        context={context}
                        onOpenAddPanel={handleOpenAddPanel}
                        view={view}
                        isScripter={isScripter}
                        />)}
                </SortableContext>
            </Accordion>
            </DndContext>

            {isSavingOrder && (
                <div className="savingOverlay">
                    <Spinner size="medium" className="footerSave" label="Saving..." labelPosition='below'/>
                </div>
            )}

        </div>
        <AddQuestionsPanelContainer
            isOpen={isPanelOpen}
            onClose={() => setIsPanelOpen(false)}
            row={selectedRow}
            existingRows={rows} //pass all existing rows (questions) on project
            onRefresh={() => refreshData()}
            addFromHeader={false}
            context={context}
            isScripter={isScripter}
            />
        </>
    );
};