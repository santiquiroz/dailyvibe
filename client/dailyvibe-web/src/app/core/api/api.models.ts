export interface Credentials {
  email: string;
  password: string;
}

export interface AuthResult {
  userId: string;
  email: string;
  token: string;
}

export interface DailyMessage {
  id: string;
  content: string;
  intent: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  size: number;
  totalCount: number;
}
