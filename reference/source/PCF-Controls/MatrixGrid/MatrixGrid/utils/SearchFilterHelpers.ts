import { ColumnItem } from "../models/ColumnItem";
import { JunctionItem } from "../models/JunctionItem";
import { RowItem } from "../models/RowItem";

export class SearchFiltersHelpers {

    static filterRowItems(items: RowItem[], searchCriteria: string, dropdownValue: string): RowItem[] {
       return this.filterByNameAndDropdown(items, searchCriteria, dropdownValue);
    }

    static filterColumnItems(items: ColumnItem[], searchCriteria: string, dropdownValue: string): ColumnItem[] {
        return this.filterByNameAndDropdown(items, searchCriteria, dropdownValue);
    }
    
    static filterJunctionItems(items: JunctionItem[], dropdownValue: string): JunctionItem[] {
        return items.filter(i => i.dropdownValueToFilter === dropdownValue);
    }

    private static filterByNameAndDropdown<T extends { name?: string; dropdownValueToFilter?: string }>(
        items: T[],
        searchCriteria: string,
        dropdownValue?: string
    ): T[] {
        const query = (searchCriteria ?? "").toLowerCase();

        return items.filter(i => {
            const matchesSearch = (i.name ?? "").toLowerCase().includes(query);
            const matchesDropdown = dropdownValue ? i.dropdownValueToFilter === dropdownValue : true;
            return matchesSearch && matchesDropdown;
        });
    }
}