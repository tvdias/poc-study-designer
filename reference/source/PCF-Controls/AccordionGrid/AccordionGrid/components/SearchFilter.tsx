import { SearchBox } from "@fluentui/react-components";
import { SearchFilterProps } from "../models/props/SearchFilterProps";
import * as React from "react";

export const SearchFilter: React.FC<SearchFilterProps> = ({
    context,
    onSearch
}) => {
    function handleSearchFilterChange(newValue: string) {
        onSearch(newValue);
    }

    return (
        <SearchBox
            size="large"
            placeholder="Search"
            onChange={(_, data) => handleSearchFilterChange(data.value)}
        />
    );
};
