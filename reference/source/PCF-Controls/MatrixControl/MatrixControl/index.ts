import { IInputs, IOutputs } from "./generated/ManifestTypes";
import * as React from "react";
import * as ReactDOM from "react-dom";
import { MatrixContainer } from "./components/MatrixContainer";
import { MatrixConfig } from "./types/MatrixTypes";
import { DataService } from "./services/DataService";

export class MatrixControl implements ComponentFramework.StandardControl<IInputs, IOutputs> {
    private container: HTMLDivElement;
    private context: ComponentFramework.Context<IInputs>;
    private dataService: DataService;
    private matrixConfig: MatrixConfig;
    private notifyOutputChanged: () => void;

    /**
     * Initialize the control
     */
    public init(
        context: ComponentFramework.Context<IInputs>,
        notifyOutputChanged: () => void,
        state: ComponentFramework.Dictionary,
        container: HTMLDivElement
    ): void {
        this.container = container;
        this.context = context;
        this.notifyOutputChanged = notifyOutputChanged;
        this.dataService = new DataService(context.webAPI, {}, context);

        // Build configuration from properties
        this.matrixConfig = this.buildMatrixConfig(context.parameters);
        
        // Render the React component
        this.renderReactComponent();
    }

    /**
     * Update the view when data changes
     */
    public updateView(context: ComponentFramework.Context<IInputs>): void {
        this.context = context;
        
        // Rebuild configuration in case properties changed
        this.matrixConfig = this.buildMatrixConfig(context.parameters);
        
        // Re-render with updated configuration
        this.renderReactComponent();
    }

    /**
     * Get outputs for the control
     */
    public getOutputs(): IOutputs {
        return {};
    }

    /**
     * Cleanup when control is destroyed
     */
    public destroy(): void {
        ReactDOM.unmountComponentAtNode(this.container);
    }

    /**
 * Build matrix configuration from PCF parameters
 */
private buildMatrixConfig(parameters: IInputs): MatrixConfig {
    console.log('=== DEBUG Entity Id Parameter ===');
    console.log('entityId raw:', parameters.entityId?.raw);
    console.log('entityId formatted:', parameters.entityId?.formatted);
    return {
        // Entity Configuration
        rowEntityName: parameters.rowEntityName?.raw || "",
        columnEntityName: parameters.columnEntityName?.raw || "",
        junctionEntityName: parameters.junctionEntityName?.raw || "",

        // Row Entity Field Mappings
        rowIdField: parameters.rowIdField?.raw || "",
        rowDisplayField: parameters.rowDisplayField?.raw || "",
        
        // Column Entity Field Mappings
        columnIdField: parameters.columnIdField?.raw || "", 
        columnDisplayField: parameters.columnDisplayField?.raw || "",
        
        // Junction Entity Field Mappings
        junctionRowField: parameters.junctionRowField?.raw || "",
        junctionColumnField: parameters.junctionColumnField?.raw || "",
        junctionIdField: parameters.junctionIdField?.raw || "",

        // Parent Relationship Fields (for form context filtering)
        rowParentField: parameters.rowParentField?.raw || "",
        columnParentField: parameters.columnParentField?.raw || "",

        // Current Record Context (NEW - from Microsoft documentation approach)
        entityId: parameters.entityId?.raw || "",
        entityName: parameters.entityName?.raw || "",

        // NEW: Hierarchy & Version Support
        columnParentAttrField: parameters.columnParentAttrField?.raw || undefined,
        columnVersionField: parameters.columnVersionField?.raw || undefined,

        // NEW: UI Configuration  
        bulkSelectionTooltip: parameters.bulkSelectionTooltip?.raw || undefined,

        // Optional Configuration
        pageSize: parameters.pageSize?.raw || 20,
        enableBatchSave: parameters.enableBatchSave?.raw ?? true,
        autoSaveDelay: undefined, // Explicit save only for this implementation

        // Additional Context for ML Forms
        parentEntityId: parameters.parentEntityId?.raw || "",
        parentEntityName: parameters.parentEntityName?.raw || "",
    };
}

    /**
     * Detect if control is in design/configuration mode
     */
    private isInDesignMode(): boolean {
        try {
            // Check if we have minimal configuration that suggests runtime usage
            const hasEntityContext = this.matrixConfig.entityId && this.matrixConfig.entityId.trim().length > 0;
            const hasBasicConfig = this.matrixConfig.rowEntityName && this.matrixConfig.columnEntityName;
            
            // If we don't have entity context or basic config, we're likely in design mode
            return !hasEntityContext || !hasBasicConfig;
        } catch {
            return true; // Assume design mode if we can't determine
        }
    }

    /**
     * Get configuration completeness status
     */
    private getConfigurationStatus(): Array<{name: string, configured: boolean, critical: boolean}> {
        return [
            { name: 'Entity Id', configured: !!(this.matrixConfig.entityId?.trim()), critical: true },
            { name: 'Entity Name', configured: !!(this.matrixConfig.entityName?.trim()), critical: true },
            { name: 'Row Entity Name', configured: !!(this.matrixConfig.rowEntityName?.trim()), critical: true },
            { name: 'Column Entity Name', configured: !!(this.matrixConfig.columnEntityName?.trim()), critical: true },
            { name: 'Junction Entity Name', configured: !!(this.matrixConfig.junctionEntityName?.trim()), critical: true },
            { name: 'Row ID Field', configured: !!(this.matrixConfig.rowIdField?.trim()), critical: true },
            { name: 'Row Display Field', configured: !!(this.matrixConfig.rowDisplayField?.trim()), critical: true },
            { name: 'Column ID Field', configured: !!(this.matrixConfig.columnIdField?.trim()), critical: true },
            { name: 'Column Display Field', configured: !!(this.matrixConfig.columnDisplayField?.trim()), critical: true },
            { name: 'Junction Row Field', configured: !!(this.matrixConfig.junctionRowField?.trim()), critical: true },
            { name: 'Junction Column Field', configured: !!(this.matrixConfig.junctionColumnField?.trim()), critical: true },
            { name: 'Row Parent Field', configured: !!(this.matrixConfig.rowParentField?.trim()), critical: true },
            { name: 'Column Parent Field', configured: !!(this.matrixConfig.columnParentField?.trim()), critical: true }
        ];
    }

    /**
     * Check if configuration is complete for runtime
     */
    private isConfigurationComplete(): boolean {
        const status = this.getConfigurationStatus();
        return status.filter(item => item.critical).every(item => item.configured);
    }

    /**
     * Get parent record ID from form context
     */
    private getParentRecordId(): string | undefined {
        try {
            // Get the current record ID from form context
            const entityId = this.matrixConfig.entityId;
            
            if (entityId && entityId.trim().length > 0) {
                console.log('Parent record ID from form context:', entityId);
                return entityId;
            }
            
            console.warn('No parent record ID found in form context');
            return undefined;
            
        } catch (error) {
            console.error('Error getting parent record ID:', error);
            return undefined;
        }
    }

    /**
     * Validate that the control has proper runtime context
     */
    private validateRuntimeContext(): {isValid: boolean, error?: string, details?: string} {
        // Check entity context
        if (!this.matrixConfig.entityId || this.matrixConfig.entityId.trim().length === 0) {
            return {
                isValid: false,
                error: "Missing Entity Id configuration",
                details: "Please configure the 'Entity Id' property and bind it to the current record's primary key field (e.g., accountid)."
            };
        }

        if (!this.matrixConfig.entityName || this.matrixConfig.entityName.trim().length === 0) {
            return {
                isValid: false,
                error: "Missing Entity Name configuration", 
                details: "Please configure the 'Entity Name' property with the entity logical name (e.g., account)."
            };
        }

        // Check if we can get parent record ID
        const parentRecordId = this.getParentRecordId();
        if (!parentRecordId) {
            return {
                isValid: false,
                error: "Unable to get parent record ID",
                details: "Make sure the 'Entity Id' property is properly bound to the current record's primary key field and that the control is placed on a record form."
            };
        }

        return { isValid: true };
    }

    /**
     * Render configuration guidance during design time
     */
    private renderConfigurationGuidance(): void {
        const configStatus = this.getConfigurationStatus();
        const configuredCount = configStatus.filter(item => item.configured).length;
        const totalCount = configStatus.length;
        
        this.container.innerHTML = `
            <div style="
                padding: 20px; 
                color: #0078d4; 
                background-color: #f3f9ff; 
                border: 2px solid #0078d4; 
                border-radius: 6px;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                font-size: 14px;
                line-height: 1.4;
                max-width: 600px;
                margin: 20px auto;
            ">
                <div style="font-weight: 600; margin-bottom: 12px;">
                    Matrix Control Configuration
                </div>
                <div style="margin-bottom: 12px;">
                    Configuration Progress: ${configuredCount}/${totalCount} properties configured
                </div>
                <div style="margin-bottom: 12px;">
                    Configure the following properties to complete setup:
                </div>
                <div style="margin-left: 20px; margin-bottom: 12px;">
                    ${configStatus.map(item => `
                        <div style="margin-bottom: 4px; color: ${item.configured ? '#107c10' : '#d13438'};">
                            ${item.configured ? 'CONFIGURED' : 'REQUIRED'}: ${item.name}
                        </div>
                    `).join('')}
                </div>
                <div style="font-size: 12px; color: #666;">
                    This control will work once all required properties are configured and placed on a record form.
                </div>
            </div>
        `;
    }

    /**
     * Render error message for runtime issues
     */
    private renderRuntimeError(error: string, details?: string): void {
        this.container.innerHTML = `
            <div style="
                padding: 20px; 
                color: #d13438; 
                background-color: #fef7f7; 
                border: 2px solid #d13438; 
                border-radius: 6px;
                font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
                font-size: 14px;
                line-height: 1.4;
                max-width: 600px;
                margin: 20px auto;
            ">
                <div style="font-weight: 600; margin-bottom: 8px;">
                    Matrix Control Runtime Error
                </div>
                <div style="margin-bottom: 12px;">
                    ${error}
                </div>
                ${details ? `
                <div style="font-size: 12px; color: #666; margin-bottom: 12px;">
                    ${details}
                </div>
                ` : ''}
                <div style="font-size: 12px; color: #666;">
                    Please check the control configuration and form context.
                </div>
            </div>
        `;
    }

    /**
     * Render the React component
     */
    private renderReactComponent(): void {
        const isDesignMode = this.isInDesignMode();
        
        // During design/configuration mode - show helpful guidance
        if (isDesignMode) {
            this.renderConfigurationGuidance();
            return;
        }

        // Runtime mode - perform strict validation
        const runtimeValidation = this.validateRuntimeContext();
        if (!runtimeValidation.isValid) {
            this.renderRuntimeError(runtimeValidation.error!, runtimeValidation.details);
            return;
        }

        // Check if configuration is complete
        if (!this.isConfigurationComplete()) {
            const configStatus = this.getConfigurationStatus();
            const missingConfig = configStatus.filter(item => item.critical && !item.configured);
            
            this.renderRuntimeError(
                "Incomplete configuration detected",
                `Missing required properties: ${missingConfig.map(item => item.name).join(", ")}`
            );
            return;
        }

        // Get parent record ID (we know it exists due to validation above)
        const parentRecordId = this.getParentRecordId()!;

        // All validation passed - render the React component
        console.log('Rendering Matrix Control with configuration:', {
            ...this.matrixConfig,
            parentRecordId
        });

        const props = {
            config: this.matrixConfig,
            dataService: this.dataService,
            context: this.context,
            onNotifyOutputChanged: this.notifyOutputChanged,
            parentRecordId // Pass parent record ID to MatrixContainer
        };

        ReactDOM.render(
            React.createElement(MatrixContainer, props),
            this.container
        );
    }
}