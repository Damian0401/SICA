import axios from 'axios';

const API_BASE_URL = 'http://localhost:5050/api/v1/files';
const COUNT = 5

export const uploadFile = async (file: File): Promise<any> => {
  const form = new FormData();
  form.append('file', file);

  try {
    const { data } = await axios.post(API_BASE_URL, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  } catch (error) {
    console.error('Upload failed:', error);
    throw error;
  }
};

export const queryFiles = async (query: string): Promise<any> => {
  try {
    const response = await fetch(`${API_BASE_URL}?Query=${encodeURIComponent(query)}&Count=${COUNT}`);
    if (!response.ok) {
      throw new Error(`Query failed: ${response.statusText}`);
    }
    const data = await response.json();
    return data;
  } catch (error) {
    console.error(error);
    throw error;
  }
};
