import * as React from 'react';
import { Button, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, useId } from '@fluentui/react-components';
import { ConfirmDialogProps } from '../models/props/ConfirmDialogProps';

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
    context,
    dialogTitle,
    dialogText,
    buttonPrimaryText,
    buttonSecondaryText,
    onPrimaryActionClick,
    onSecondaryActionClick
}) => {
    const dialogId = useId("dialog-");

    return (
        <DialogSurface
            aria-labelledby={`${dialogId}-title`}
            aria-describedby={`${dialogId}-content`}
        >
            <DialogBody>
            <DialogTitle id={`${dialogId}-title`}>
                {dialogTitle}
            </DialogTitle>
            <DialogContent id={`${dialogId}-content`}>
                {dialogText}
            </DialogContent>
            <DialogActions>
                <DialogTrigger disableButtonEnhancement>
                <Button appearance="primary" onClick={onPrimaryActionClick}>{buttonPrimaryText}</Button>
                </DialogTrigger>
                {buttonSecondaryText && buttonSecondaryText.trim() !== "" && (
                    <DialogTrigger disableButtonEnhancement>
                        <Button appearance="secondary" onClick={onSecondaryActionClick}>
                            {buttonSecondaryText}
                        </Button>
                        </DialogTrigger>
                    )}
            </DialogActions>
            </DialogBody>
        </DialogSurface>
    );
};