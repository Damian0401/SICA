import { useState, useEffect } from "react";
import { CV } from "../types";
import { useToast } from "@/components/ui/use-toast";
import { fileService, FileResponse } from "../services/fileService";

export function useCVs() {
  const [cvs, setCVs] = useState<CV[]>([]);
  const [loading, setLoading] = useState(true);
  const { toast } = useToast();

  const loadCVs = async () => {
    setLoading(true);
    try {
      // Get files from API
      const apiFiles = await fileService.getFiles();
      
      // Map API files to CV format
      const apiCVs: CV[] = apiFiles.map((file: FileResponse) => {
        // Safely convert date strings to Date objects
        let uploadDate: Date;
        
        try {
          // Try to use createdAt from the API response
          if (file.createdAt) {
            uploadDate = new Date(file.createdAt);
            // Validate if the date is valid
            if (isNaN(uploadDate.getTime())) {
              throw new Error("Invalid date from createdAt");
            }
          } 
          // Fallback to uploadDate if createdAt is not available
          else if (file.uploadDate) {
            uploadDate = new Date(file.uploadDate);
            // Validate if the date is valid
            if (isNaN(uploadDate.getTime())) {
              throw new Error("Invalid date from uploadDate");
            }
          } 
          // Use current date as last resort
          else {
            uploadDate = new Date();
          }
        } catch (error) {
          console.error("Error parsing date:", error);
          // Default to current date if parsing fails
          uploadDate = new Date();
        }

        return {
          id: file.id,
          name: file.name || file.fileName, // Use fileName if name is not available
          fileName: file.fileName,
          summary: file.summary || "",
          uploadDate,
          // The actual content is not loaded initially
        };
      });
      
      setCVs(apiCVs);
    } catch (error) {
      console.error('Failed to load CVs from API:', error);
      toast({
        title: "Connection Issue",
        description: "Couldn't connect to CV service. Please try again later.",
        variant: "destructive"
      });
      // Initialize with empty array on error
      setCVs([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCVs();
  }, []);

  const uploadCV = async (file: File): Promise<boolean> => {
    try {
      // Upload to the API
      const uploadSuccessful = await fileService.uploadFiles([file]);
      
      if (!uploadSuccessful) {
        toast({
          title: "Upload Failed",
          description: "Failed to upload the CV to the server",
          variant: "destructive",
        });
        return false;
      }
      
      // Refresh the CV list after successful upload
      await loadCVs();
      
      toast({
        title: "CV Uploaded",
        description: `${file.name} has been uploaded successfully`,
      });
      
      return true;
    } catch (error) {
      console.error("Error uploading CV:", error);
      toast({
        title: "Upload Failed",
        description: "An error occurred while uploading the CV",
        variant: "destructive",
      });
      return false;
    }
  };

  const deleteCV = async (id: string) => {
    try {
      // Call the API to delete the CV
      const deleteSuccessful = await fileService.removeFile(id);
      
      if (!deleteSuccessful) {
        toast({
          title: "Delete Failed",
          description: "Failed to delete the CV from the server",
          variant: "destructive",
        });
        return false;
      }
      
      // Remove from local state after successful API deletion
      setCVs((prev) => prev.filter((cv) => cv.id !== id));
      
      toast({
        title: "CV Deleted",
        description: "The CV has been removed successfully",
      });
      
      return true;
    } catch (error) {
      console.error("Error deleting CV:", error);
      toast({
        title: "Delete Failed",
        description: "An error occurred while deleting the CV",
        variant: "destructive",
      });
      return false;
    }
  };

  return {
    cvs,
    loading,
    uploadCV,
    deleteCV,
    refreshCVs: loadCVs,
  };
}
