import { CV } from "../types";

const API_URL = "http://localhost:5050";

export interface FileResponse {
  id: string;
  fileName: string;
  contentLanguage?: string;
  createdAt: string;
  name?: string;
  uploadDate?: string;
  url?: string;
  score?: number;
  summary?: string;
}

export interface FilesApiResponse {
  files: FileResponse[];
  limit: number;
  offset: number;
  count: number;
}

// Mock response for development and testing
const MOCK_RESPONSE: FilesApiResponse = {
  "files":[
    {
      "id":"0196bfd2-bfb6-7b16-a05b-2dc7878d9c64",
      "fileName":"SZYMON LEJA_CV.pdf",
      "summary":"Experienced software developer with a focus on backend technologies and cloud solutions. Proficient in Java, Spring Boot, and AWS. Strong problem-solving skills and a passion for clean code.",
      "name":"SZYMON LEJA",
      "contentLanguage":"en-US,en;q=0.9,pl-PL;q=0.8,pl;q=0.7",
      "createdAt":"2025-05-11T14:49:40.2476801+00:00"
    },
    {
      "id":"0196bfd3-eb0b-7a00-a1f9-63e102447429",
      "fileName":"SZYMON LEJA_CV.pdf",
      "contentLanguage":"en-US,en;q=0.9,pl-PL;q=0.8,pl;q=0.7",
      "createdAt":"2025-05-11T14:50:57.6736688+00:00"
    },
    {
      "id":"0196bfd4-e590-75ac-8e50-9b340bce420b",
      "fileName":"Porównanie stylów API_ REST, GraphQL, WebSocket, gRPC, Server-Sent Events oraz architektury zdarzeni (1).pdf",
      "contentLanguage":"en-US,en;q=0.9,pl-PL;q=0.8,pl;q=0.7",
      "createdAt":"2025-05-11T14:51:59.1781436+00:00"
    }
  ],
  "limit":3,
  "offset":0,
  "count":3
};

// Flag to use mock data instead of actual API calls
const USE_MOCK = false;

export const fileService = {
  /**
   * Upload files to the API
   */
  uploadFiles: async (files: File[]): Promise<boolean> => {
    try {
      const formData = new FormData();
      
      // Add each file to the FormData object with the key 'Files'
      files.forEach((file) => {
        formData.append('Files', file);
      });

      const response = await fetch(`${API_URL}/api/v1/files`, {
        method: 'POST',
        body: formData,
        // No need to set Content-Type as it's automatically set with the correct boundary
      });

      if (!response.ok) {
        throw new Error(`Upload failed: ${response.status}`);
      }

      return true;
    } catch (error) {
      console.error('Error uploading files:', error);
      return false;
    }
  },

  /**
   * Get files with pagination
   */
  getFiles: async (limit: number = 10, offset: number = 0): Promise<FileResponse[]> => {
    try {
      if (USE_MOCK) {
        console.log("Using mock data:", MOCK_RESPONSE);
        return MOCK_RESPONSE.files;
      }

      const response = await fetch(
        `${API_URL}/api/v1/files?Limit=10`
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch files: ${response.status}`);
      }

      const data = await response.json() as FilesApiResponse;
      console.log("API response:", data);
      
      return data.files || [];
    } catch (error) {
      console.error('Error getting files:', error);
      throw error; // Re-throw to handle in the calling component
    }
  },

  /**
   * Search for files by query
   */
  searchFiles: async (query: string, limit: number = 3): Promise<FileResponse[]> => {
    try {
      const response = await fetch(
        `${API_URL}/api/v1/files/search?Query=${encodeURIComponent(query)}&Limit=${limit}`
      );

      if (!response.ok) {
        throw new Error(`Search failed: ${response.status}`);
      }

      const data = await response.json() as FilesApiResponse;
      console.log("Search API response:", data);
      return data.files || [];
    } catch (error) {
      console.error('Error searching files:', error);
      throw error;
    }
  },

  /**
   * Get download URL for a file
   */
  getFileDownloadUrl: (fileId: string): string => {
    return `${API_URL}/api/v1/files/${fileId}/download`;
  },

  /**
   * Remove a file by ID
   */
  removeFile: async (fileId: string): Promise<boolean> => {
    try {
      const response = await fetch(`${API_URL}/api/v1/files/${fileId}`, {
        method: 'DELETE',
      });

      if (!response.ok) {
        throw new Error(`File deletion failed: ${response.status}`);
      }

      return true;
    } catch (error) {
      console.error('Error removing file:', error);
      return false;
    }
  },
  
};
