// Mock for react/jsx-runtime
const React = require('react');

module.exports = {
  jsx: React.createElement,
  jsxs: React.createElement,
  Fragment: React.Fragment
};