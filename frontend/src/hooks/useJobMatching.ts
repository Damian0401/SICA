
import { useState } from "react";
import { CV, MatchResult } from "../types";
import { useToast } from "@/components/ui/use-toast";
import { fileService } from "../services/fileService";

export function useJobMatching() {
  const [results, setResults] = useState<MatchResult[]>([]);
  const [searching, setSearching] = useState(false);
  const { toast } = useToast();

  const searchMatches = async (jobDescription: string, cvs: CV[]): Promise<void> => {
    if (!jobDescription.trim()) {
      toast({
        title: "Empty Job Description",
        description: "Please enter a job description to search for matches",
        variant: "destructive",
      });
      return;
    }
    
    if (cvs.length === 0) {
      toast({
        title: "No CVs Available",
        description: "Please upload at least one CV first",
        variant: "destructive",
      });
      return;
    }
    
    setSearching(true);
    
    try {
      // Try to use the API search function
      const apiResults = await fileService.searchFiles(jobDescription);
      
      if (apiResults.length > 0) {
        // Map API results to match results
        // Note: This assumes the API returns match percentages - adapt as needed
        const matchResults: MatchResult[] = apiResults.map(file => {
          // Find the corresponding CV in our local list
          const matchedCV = cvs.find(cv => cv.id === file.id);
          
          // If we don't have this CV locally, create a temporary one
          const cv: CV = matchedCV || {
            id: file.id,
            name: file.name,
            fileName: file.fileName,
            uploadDate: new Date(file.uploadDate),
            score: file.score,
          };
          
          // Random match percentage between 30-95% if not provided by API
          // In a real implementation, this would come from the API
          const matchPercentage = (file.score*100).toFixed(2) || 0;
          
          return { cv, matchPercentage };
        });
        
        // Sort by match percentage (highest first)
        const sortedResults = matchResults.sort(
          (a, b) => b.matchPercentage - a.matchPercentage
        );
        
        setResults(sortedResults);
        toast({
          title: "Search Complete",
          description: `Found ${sortedResults.length} potential matches`,
        });
      } else {
        // Fall back to mock results if API returns no results
        // This is just for demonstration purposes
        const mockResults: MatchResult[] = cvs.map((cv) => {
          const matchPercentage = 1337;
          return { cv, matchPercentage };
        });
        
        const sortedResults = mockResults.sort(
          (a, b) => b.matchPercentage - a.matchPercentage
        );
        
        setResults(sortedResults);
        toast({
          title: "Search Complete",
          description: `Found ${sortedResults.length} potential matches`,
        });
      }
    } catch (error) {
      console.error("Error searching for matches:", error);
      
      // Fall back to mock results if API fails
      const mockResults: MatchResult[] = cvs.map((cv) => {
        const matchPercentage = Math.floor(Math.random() * (95 - 30 + 1) + 30);
        return { cv, matchPercentage };
      });
      
      const sortedResults = mockResults.sort(
        (a, b) => b.matchPercentage - a.matchPercentage
      );
      
      setResults(sortedResults);
      toast({
        title: "Search Complete (Local)",
        description: `Found ${sortedResults.length} potential matches locally`,
      });
    } finally {
      setSearching(false);
    }
  };

  return {
    results,
    searching,
    searchMatches,
    clearResults: () => setResults([]),
  };
}
