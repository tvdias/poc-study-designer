export enum StandardOrCustom {
   Standard = 0,
   Custom = 1,
}

export enum SortOrder {
   Alphabetical = 847610000,
   Random = 847610001,
   SharedRandom = 847610002,
   Normal = 847610003,
   Rotated = 847610004
}

export const StandardOrCustomStyles: Record<StandardOrCustom, React.CSSProperties> = {
   [StandardOrCustom.Standard]: {
      backgroundColor: '#0060FF',
      color: '#EDF8FF',
   },
   [StandardOrCustom.Custom]: {
      color: '#F7E8FF',
      backgroundColor: '#F826FF'
   }
}