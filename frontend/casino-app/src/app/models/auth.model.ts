export interface User {
  userId: number;
  username: string;
  email: string;
  balance: number;
  createdDate: string;
  lastLoginDate?: string;
}

export interface LoginRequest {
  usernameOrEmail: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface TokenResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  expiresAt?: string;
  user: User;
}

export interface ApiError {
  message: string;
  errors?: { [key: string]: string[] };
}
