
import React from 'react';

const Header: React.FC = () => {
  return (
    <header className="py-6 border-b border-gray-100">
      <div className="app-container">
        <h1 className="text-2xl font-medium text-gray-900">CV Match</h1>
        <p className="text-sm text-gray-500 mt-1">
          Find the perfect candidate for your job description
        </p>
      </div>
    </header>
  );
};

export default Header;
