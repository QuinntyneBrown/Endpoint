export enum OutputType {
  DotNetSolution = 'DotNetSolution',
  AngularWorkspace = 'AngularWorkspace',
  ReactApp = 'ReactApp',
  Custom = 'Custom'
}

export interface FolderConfiguration {
  sourcePath: string;
  destinationPath: string;
}

export interface RepositoryConfiguration {
  url: string;
  branch: string;
  folders: FolderConfiguration[];
  isLocal?: boolean;
}

export interface ALaCarteRequest {
  id: string;
  name: string;
  solutionName: string;
  outputDirectory: string;
  outputType: OutputType;
  repositories: RepositoryConfiguration[];
  createdAt: Date;
  lastUsed?: Date;
  executionCount: number;
}

export interface ALaCarteRequestExecution {
  requestId: string;
  startedAt: Date;
  completedAt?: Date;
  status: 'queued' | 'in_progress' | 'completed' | 'failed';
  currentStage?: string;
  logs: ExecutionLog[];
}

export interface ExecutionLog {
  timestamp: Date;
  level: 'info' | 'success' | 'warning' | 'error';
  message: string;
}
