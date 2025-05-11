
import React from 'react';
import { MatchResult } from '@/types';
import { Progress } from '@/components/ui/progress';
import { FileText } from 'lucide-react';
import { format } from 'date-fns';

interface MatchResultsProps {
  results: MatchResult[];
}

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
              <p className="text-xs text-gray-500">
                {result.cv.fileName} • Uploaded on {format(result.cv.uploadDate, 'MMM d, yyyy')}
              </p>
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
