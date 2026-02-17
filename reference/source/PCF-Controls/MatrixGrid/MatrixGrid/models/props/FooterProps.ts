export interface FooterProps {
  pendingChangesCount: number;
  isSaving: boolean;
  onSave: () => void;
  onCancel: () => void;
  disabled: boolean;
}