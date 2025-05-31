
import React, { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Search } from 'lucide-react';
import { useCVs } from '@/hooks/useCVs';
import { useJobMatching } from '@/hooks/useJobMatching';
import MatchResults from './MatchResults';

const JobDescriptionForm: React.FC = () => {
  const [jobDescription, setJobDescription] = useState('');
  const { cvs } = useCVs();
  const { results, searching, searchMatches } = useJobMatching();

  const handleSearch = async () => {
    await searchMatches(jobDescription, cvs);
  };
  
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-medium text-gray-900 mb-1">Job Description</h2>
        <p className="text-sm text-gray-500 mb-4">
          Enter a job description to find matching candidates
        </p>
      </div>
      
      <div className="space-y-4">
        <Textarea
          placeholder="Enter job requirements, skills, experience needed..."
          className="min-h-[200px] resize-y"
          value={jobDescription}
          onChange={(e) => setJobDescription(e.target.value)}
        />
        
        <div className="flex justify-end">
          <Button
            onClick={handleSearch}
            className="apple-button-primary"
            disabled={searching || !jobDescription.trim()}
          >
            {searching ? (
              "Searching..."
            ) : (
              <>
                <Search className="h-4 w-4 mr-2" />
                Find Matches
              </>
            )}
          </Button>
        </div>
      </div>
      
      <MatchResults results={results} />
    </div>
  );
};

export default JobDescriptionForm;
