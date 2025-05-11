
export type CV = {
  id: string;
  name: string;
  fileName: string;
  uploadDate: Date;
  content?: string;
  url?: string;
  score?: number;
};

export type MatchResult = {
  cv: CV;
  matchPercentage: number;
};

export type ApiResponse<T> = {
  success: boolean;
  data?: T;
  error?: string;
};

export type PaginationParams = {
  limit: number;
  offset: number;
};

export type SearchParams = {
  query: string;
  limit?: number;
};
