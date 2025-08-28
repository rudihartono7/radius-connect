# RadiusConnect Frontend

A modern Vue.js frontend application for managing RADIUS infrastructure, built with Nuxt 3.

## Features

- **Modern UI**: Built with Tailwind CSS and Headless UI components
- **Authentication**: JWT-based authentication with role-based access control
- **Dashboard**: Real-time system overview and statistics
- **User Management**: Complete CRUD operations for system users
- **RADIUS Management**: Manage RADIUS users, groups, and sessions
- **Audit Logging**: Comprehensive audit trail and reporting
- **Responsive Design**: Mobile-friendly interface

## Tech Stack

- **Framework**: Nuxt 3 (Vue 3)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: Pinia
- **UI Components**: Headless UI + Heroicons
- **Charts**: Chart.js with vue-chartjs
- **Package Manager**: Bun

## Project Structure

```
ClientApp/
├── components/
│   ├── ui/           # Reusable UI components
│   ├── layout/       # Layout components
│   ├── forms/        # Form components
│   └── charts/       # Chart components
├── pages/            # Application pages
│   ├── users/        # User management pages
│   ├── groups/       # Group management pages
│   ├── sessions/     # Session management pages
│   ├── audit/        # Audit log pages
│   └── settings/     # Settings pages
├── composables/      # Vue composables
├── stores/           # Pinia stores
├── middleware/       # Route middleware
├── plugins/          # Nuxt plugins
├── utils/            # Utility functions
├── types/            # TypeScript type definitions
└── assets/           # Static assets
```

## Getting Started

### Prerequisites

- Node.js 18+ or Bun
- RadiusConnect API running on `http://localhost:5000`

### Installation

1. Install dependencies:
   ```bash
   bun install
   ```

2. Set up environment variables:
   ```bash
   # Create .env file
   NUXT_PUBLIC_API_BASE=http://localhost:5000/api
   ```

3. Start the development server:
   ```bash
   bun run dev
   ```

4. Open your browser and navigate to `http://localhost:3000`

### Building for Production

```bash
# Build the application
bun run build

# Preview the production build
bun run preview
```

## API Integration

The frontend integrates with the RadiusConnect API through the following controllers:

- **AuthController**: Authentication and user management
- **DashboardController**: System overview and statistics
- **UsersController**: User CRUD operations
- **RadiusController**: RADIUS user and group management
- **AuditController**: Audit logging and reporting

## Authentication Flow

1. User logs in with username/password
2. If 2FA is enabled, TOTP code is required
3. JWT tokens are stored in secure cookies
4. Automatic token refresh on expiration
5. Role-based route protection

## Available Pages

- `/` - Dashboard overview
- `/login` - Authentication page
- `/users` - User management
- `/groups` - RADIUS group management
- `/sessions` - Active session monitoring
- `/audit` - Audit log viewer
- `/settings` - Application settings

## Development

### Adding New Pages

1. Create a new Vue file in the `pages/` directory
2. Use the `definePageMeta()` to set middleware and metadata
3. Implement the page logic using composables and stores

### Adding New Components

1. Create components in the appropriate `components/` subdirectory
2. Use TypeScript interfaces for props and emits
3. Follow the established naming conventions

### API Integration

1. Add new API endpoints to `utils/api.ts`
2. Create corresponding store actions
3. Use the `useApi` composable for data fetching

## Contributing

1. Follow the existing code style and conventions
2. Use TypeScript for all new code
3. Add proper error handling and loading states
4. Test your changes thoroughly

## License

This project is part of the RadiusConnect application suite.
