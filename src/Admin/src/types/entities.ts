export interface BaseEntity {
  id: number;
  createdAt: string;
  createdBy: string;
  modifiedAt?: string;
  modifiedBy?: string;
  isActive: boolean;
}

export interface VersionedEntity extends BaseEntity {
  version: number;
  status: string;
}

export interface Tag extends BaseEntity {
  name: string;
  description?: string;
}

export interface Client extends BaseEntity {
  name: string;
  integrationProperties?: string;
}

export interface CommissioningMarket extends BaseEntity {
  name: string;
  isoCode: string;
}

export interface FieldworkMarket extends BaseEntity {
  name: string;
  isoCode: string;
}

export interface Question extends VersionedEntity {
  variableName: string;
  title: string;
  text: string;
  type: string;
  methodology?: string;
  isStandard: boolean;
  isDummy: boolean;
  scriptNotes?: string;
  metricGroup?: string;
  dataQualityTags?: string;
  tableNotes?: string;
  scale?: string;
  displayType?: string;
  restrictions?: string;
  facets?: string;
  parentQuestionId?: number;
  isTranslatable: boolean;
  isHidden: boolean;
  answers?: QuestionAnswer[];
  questionTags?: { tag: Tag }[];
}

export interface QuestionAnswer extends VersionedEntity {
  questionId: number;
  text: string;
  code: string;
  location: string;
  isFixed: boolean;
  isExclusive: boolean;
  isOpen: boolean;
  isTranslatable: boolean;
  displayOrder: number;
  facets?: string;
  restrictions?: string;
  ruleMetadata?: string;
}

export interface Module extends VersionedEntity {
  variableName: string;
  label: string;
  description?: string;
  instructions?: string;
  parentModuleId?: number;
  moduleQuestions?: { question: Question; displayOrder: number }[];
}

export interface Product extends VersionedEntity {
  name: string;
  description?: string;
  rules?: string;
  productTemplates?: ProductTemplate[];
}

export interface ProductTemplate extends VersionedEntity {
  productId: number;
  name: string;
  templateData?: string;
}

export interface ConfigurationQuestion extends VersionedEntity {
  question: string;
  rule: string;
  aiPrompt?: string;
  dependencyRules?: string;
  answers?: ConfigurationQuestionAnswer[];
}

export interface ConfigurationQuestionAnswer extends BaseEntity {
  configurationQuestionId: number;
  text: string;
  code: string;
  displayOrder: number;
}
