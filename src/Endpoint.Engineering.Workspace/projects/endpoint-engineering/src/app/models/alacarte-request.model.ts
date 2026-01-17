export interface ALaCarteRequest {
  id: string;
  name: string;
  solutionName: string;
  outputDirectory: string;
  outputType: OutputType;
  description?: string;
  repositories: RepositoryConfig[];
  status: RequestStatus;
  createdAt: Date;
  updatedAt: Date;
  lastExecutedAt?: Date;
}

export interface RepositoryConfig {
  id: string;
  type: RepositoryType;
  url: string;
  branch?: string;
  folders: FolderConfig[];
  order: number;
}

export interface FolderConfig {
  id: string;
  path: string;
  order: number;
}

export type OutputType = 'dotnet' | 'angular' | 'node' | 'python' | 'custom';
export type RepositoryType = 'github' | 'local' | 'git';
export type RequestStatus = 'draft' | 'ready' | 'executing' | 'completed' | 'failed';

export interface Configuration {
  id: string;
  name: string;
  description?: string;
  repositories: RepositoryConfig[];
  createdAt: Date;
  updatedAt: Date;
}

export interface CompositionInput {
  solutionName: string;
  outputDirectory: string;
  description?: string;
  targetFramework: string;
  includeTests: boolean;
  generateDocs: boolean;
  addDocker: boolean;
  addCICD: boolean;
}

export interface ExecutionResult {
  requestId: string;
  status: 'success' | 'error';
  outputPath?: string;
  errors?: string[];
  warnings?: string[];
  generatedFiles?: string[];
  executionTime?: number;
}
