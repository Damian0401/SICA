
import React from 'react';
import { CV, MatchResult } from '@/types';
import { Progress } from '@/components/ui/progress';
import { Download, FileText, Info } from 'lucide-react';
import { format } from 'date-fns';
import { fileService } from '@/services/fileService';
import { Button } from '../ui/button';
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '../ui/tooltip';

interface MatchResultsProps {
  results: MatchResult[];
}

  const handleDownload = (cv: CV) => {
    // Only attempt download for files that have an ID from the backend
    if (cv.id) {
      const downloadUrl = fileService.getFileDownloadUrl(cv.id);
      window.open(downloadUrl, '_blank');
    }
  };

const MatchResults: React.FC<MatchResultsProps> = ({ results }) => {
  if (results.length === 0) {
    return null;
  }

  return (
    <div className="mt-8 space-y-4">
      <h3 className="text-base font-medium text-gray-700">Match Results</h3>
      
      <div className="space-y-3">
        {results.map((result) => (
          <div
            key={result.cv.id}
            className="apple-card p-4 flex flex-col sm:flex-row sm:items-center gap-4"
          >
            <div className="flex-1">
              <div className="flex items-center mb-2">
                <FileText className="h-4 w-4 text-primary mr-2" />
                <h4 className="font-medium text-gray-900">{result.cv.name}</h4>
              </div>
              <div className="flex items-center justify-between mb-1">
                <p className="text-xs text-gray-500">
                  {result.cv.fileName} • Uploaded on {format(result.cv.uploadDate, 'MMM d, yyyy')}  • 
                </p>
                <div className="inline-flex">
                  {result.cv.id && (
                    <Button
                      variant="ghost"
                      size="icon"
                      onClick={() => handleDownload(result.cv)}
                      className="h-8 w-8 text-gray-400 hover:text-primary mr-1"
                      aria-label={`Download ${result.cv.name}`}
                    >
                      <Download className="h-4 w-4" />
                    </Button>
                  )}
                  {result.cv.summary && (
                    <TooltipProvider>
                      <Tooltip>
                        <TooltipTrigger className="flex h-8 w-8 items-center justify-center text-gray-400 hover:text-primary">
                          <Info className="h-4 w-4" />
                        </TooltipTrigger>
                        <TooltipContent side='bottom' className="max-w-xs">
                          <p className="text-sm text-gray-700">{result.cv.summary}</p>
                        </TooltipContent>
                      </Tooltip>
                    </TooltipProvider>            
                  )}
                </div>
              </div>
            </div>
            
            <div className="sm:w-1/3 space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-xs text-gray-500">Match</span>
                <span 
                  className={`text-sm font-medium ${
                    result.matchPercentage >= 70 ? 'text-green-600' : 
                    result.matchPercentage >= 50 ? 'text-amber-600' : 'text-gray-600'
                  }`}
                >
                  {result.matchPercentage}%
                </span>
              </div>
              <Progress 
                value={result.matchPercentage} 
                className={`h-1.5 ${
                  result.matchPercentage >= 70 ? 'bg-green-100' : 
                  result.matchPercentage >= 50 ? 'bg-amber-100' : 'bg-gray-100'
                }`} 
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default MatchResults;
