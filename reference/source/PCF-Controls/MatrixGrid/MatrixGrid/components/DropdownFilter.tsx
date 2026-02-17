import { Select } from "@fluentui/react-components";
import { DropdownFilterProps } from "../models/props/DropdownFilterProps";
import * as React from "react";

export const DropdownFilter: React.FC<DropdownFilterProps> = ({
    onDropdownFilterChange,
    dropdownItems,
    selectedValue
}) => {
    console.log('DropdownFilter rendered with selectedValue:', selectedValue);

    return (
        <Select
            appearance="underline"
            size="large"
            value={selectedValue}
            onChange={(e, data) => onDropdownFilterChange(data.value)}
        >
            {dropdownItems.map(item => (
                <option key={item.id} value={item.id}>{item.name}</option>
            ))}
        </Select>
    );
}
