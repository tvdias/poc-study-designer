import * as React from 'react';
import { GridContainerProps } from "../models/props/GridContainerProps";
import { FluentProvider, webLightTheme,  Divider } from '@fluentui/react-components';
import { Header } from './Header';
import { ViewType } from '../types/ViewType';
import { ExpandableGrid } from './ExpandableGrid';
import {UserRoleService} from '../services/UserRoleService';
type View = ViewType.All | ViewType.Active | ViewType.Inactive  | ViewType.Dummy;


const VIEW_TO_STATUS_CODE: Record<ViewType.Active | ViewType.Inactive, number> = {
    [ViewType.Active]: 1,
    [ViewType.Inactive]: 2,
};
export const GridContainer: React.FC<GridContainerProps> = ({
    context,
    onNotifyOutputChanged,
    dataItems,
    dataService,
    isReadOnly
}) => {
    const [view, setView] = React.useState<View>(ViewType.Active);
    const [search, setSearch] = React.useState("");
    const [isScripter, setIsScripter] = React.useState(false);

        React.useMemo(() => {
            const checkRole = async () => {
                const result = await UserRoleService.CheckIfUserHasOnlyScripterRole(context.userSettings.userId);
                setIsScripter(result);
            };
            checkRole();
        }, [context.userSettings.userId]);  console.log("==> context", context);
    console.log('==> context.parameters', context.parameters);
    
    const entityName = context.parameters.gridDataSet.getTargetEntityType();
    console.log('entityName ', entityName);
const filteredDataItems = dataItems
    .filter(dataItem => {
        const query = (search ?? "").toLowerCase();
        const name = (dataItem.name ?? "").toLowerCase();
        const lastLabelText = (dataItem.lastLabelText ?? "").toLowerCase();

        return (
            name.includes(query) ||
            lastLabelText.includes(query)
        );
    })
    .filter(dataItem => {
        const statusCode = dataItem.statusCode ?? null;

        switch (view) {
            case ViewType.All:
                return true; // No filtering on status
            case ViewType.Active:
            case ViewType.Inactive:
                return statusCode === VIEW_TO_STATUS_CODE[view];
            case ViewType.Dummy:
              return String(dataItem.isDummy).toLowerCase() === "true";
            default:
                return false;
        }
    })
  .sort((a, b) => (a.sortOrder) - (b.sortOrder));


    function updateView(view: View) {
        setView(view);
    }

    function updateSearchFilter(text: string) {
        setSearch(text);
    }

    return (
    <FluentProvider theme={webLightTheme}>
        <div>
            <Header 
                context={context}
                view={view}
                updateView={updateView} 
                onSearch={updateSearchFilter}
                isReadOnly={isReadOnly}
                rows={filteredDataItems}
                entityName={entityName}                
                isScripter={isScripter}
            />

            <Divider />

            <ExpandableGrid
                context={context}
                rows={[...filteredDataItems]}
                dataService={dataService}
                entityName={entityName}
                isReadOnly={isReadOnly}
                view={view}               
                isScripter={isScripter}
            ></ExpandableGrid>
        </div>
    </FluentProvider>
    );
};