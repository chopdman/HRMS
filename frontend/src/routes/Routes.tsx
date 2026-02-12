import { createBrowserRouter } from "react-router-dom";
import { ProtectedRoute, GuestRoute, RoleRoute } from "./protected-route";
import { LoginPage } from "../pages/LoginPage";
import { Home } from "../components/Home";
import { DashboardPage } from "../pages/DashboardPage";
import { RegisterPage } from "../pages/RegisterPage";
import { AccessDeniedPage } from "../pages/AccessDeniedPage";
import { NotFoundPage } from "../pages/NotFoundPage";
import { TravelsPage } from "../pages/travel/TravelPage";
import { ExpensesPage } from "../pages/travel/ExpensesPage";
import { DocumentsPage } from "../pages/travel/DocumentsPage";
import { NotificationsPage } from "../pages/travel/NotificationsPage";
import { HrReviewsPage } from "../pages/travel/HrReviewsPage";
// import { DocumentsPage } from '../pages/documents/DocumentsPage'
// import { ExpensesPage } from '../pages/expenses/ExpensesPage'
// import { HrReviewsPage } from '../pages/hr/HrReviewsPage'
// import { ExpenseConfigPage } from '../pages/hr/ExpenseConfigPage'
// import { TeamExpensesPage } from '../pages/manager/TeamExpensesPage'
// import { TeamMembersPage } from '../pages/manager/TeamMembersPage'
// import { NotificationsPage } from '../pages/notifications/NotificationsPage'

export const router = createBrowserRouter([
  {
    path: "/login",
    element: (
      <GuestRoute>
        <LoginPage />
      </GuestRoute>
    ),
  },
    {
      path: '/register',
      element: (
        <GuestRoute>
          <RegisterPage />
        </GuestRoute>
      )
    },
    {
      path: '/access-denied',
      element: <AccessDeniedPage />
    },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <Home />
      </ProtectedRoute>
    ),
    children: [
      {
        index: true,
        element: <DashboardPage />,
      },
        {
          path: 'travels',
          element: (
            <RoleRoute allowedRoles={['Employee', 'Manager', 'HR']}>
              <TravelsPage />
            </RoleRoute>
          )
        },
        {
          path: 'expenses',
          element: (
            <RoleRoute allowedRoles={['Employee', 'HR']}>
              <ExpensesPage />
            </RoleRoute>
          )
        },
        {
          path: 'documents',
          element: (
            <RoleRoute allowedRoles={['Employee', 'Manager', 'HR']}>
              <DocumentsPage />
            </RoleRoute>
          )
        },
        {
          path: 'notifications',
          element: (
            <RoleRoute allowedRoles={['Employee', 'Manager', 'HR']}>
              <NotificationsPage />
            </RoleRoute>
          )
        },
      //   {
      //     path: 'manager/team-members',
      //     element: (
      //       <RoleRoute allowedRoles={['Manager']}>
      //         <TeamMembersPage />
      //       </RoleRoute>
      //     )
      //   },
      //   {
      //     path: 'manager/team-expenses',
      //     element: (
      //       <RoleRoute allowedRoles={['Manager']}>
      //         <TeamExpensesPage />
      //       </RoleRoute>
      //     )
      //   },
        {
          path: 'hr/reviews',
          element: (
            <RoleRoute allowedRoles={['HR']}>
              <HrReviewsPage />
            </RoleRoute>
          )
        },
      //   {
      //     path: 'hr/expense-config',
      //     element: (
      //       <RoleRoute allowedRoles={['HR']}>
      //         <ExpenseConfigPage />
      //       </RoleRoute>
      //     )
      //   }
    ],
  },
    {
      path: '*',
      element: <NotFoundPage />
    }
]);
