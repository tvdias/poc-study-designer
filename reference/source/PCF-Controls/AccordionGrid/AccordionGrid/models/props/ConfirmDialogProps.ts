export interface ConfirmDialogProps {
  context: ComponentFramework.Context<any>;
  dialogTitle: string;
  dialogText: string;
  buttonPrimaryText: string;
  buttonSecondaryText: string;
  onPrimaryActionClick: (e: React.MouseEvent<HTMLButtonElement>) => void;
  onSecondaryActionClick: (e: React.MouseEvent<HTMLButtonElement>) => void;
}