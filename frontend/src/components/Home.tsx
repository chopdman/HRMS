import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { Button } from './ui/Button'
import { IoLogOutOutline } from 'react-icons/io5'

const linkBase =
  'flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-semibold transition hover:bg-brand-50'

const linkActive = 'bg-brand-50 text-brand-700 bg-white'

export const Home = () => {
  const { logout, role, fullName, email } = useAuth()
  const isManager = role === 'Manager'
  const isHr = role === 'HR'

  return (
    <div className="min-h-screen bg-slate-50 text-(--color-text)">
      <header className="border-b border-slate-900 bg-(--color-primary)">
        <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-6 py-4">
          <div>
            <p className="text-sm font-semibold text-brand-600">HRMS</p>
            <h1 className="text-lg font-semibold">Travel & Expense</h1>
          </div>
           <Button  variant="dark" className='border bg-(--color-text) text-(--color-dark) ' onClick={logout}>
            Sign out<IoLogOutOutline color='red' size={20} />
          </Button>
        </div>
      </header>

      <div className="mx-auto flex w-full max-w-6xl gap-6 px-6 py-8">
        <aside className="hidden w-64 shrink-0 flex-col gap-6 md:flex">
          <div className="rounded-2xl border border-slate-200 bg-(--color-light) text-(--color-dark) p-4 shadow-sm">
            <p className="text-xs font-semibold uppercase tracking-wide ">Signed in</p>
            <p className="mt-2 text-sm font-semibold ">{fullName || 'HRMS User'}</p>
            <p className="text-xs ">{email || 'user@company.com'}</p>
            <span className="mt-3 inline-flex rounded-full bg-slate-100 px-3 py-1 text-xs font-semibold text-slate-700">
              {role}
            </span>
          </div>

          <nav className="rounded-2xl border border-slate-200 bg-(--color-light) text-(--color-dark) p-2 shadow-sm">
            <NavLink to="/" end className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}>
              Dashboard
            </NavLink>
            <NavLink to="/travels" className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}>
              Travels
            </NavLink>
            <NavLink to="/expenses" className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}>
              Expenses
            </NavLink>
            <NavLink to="/documents" className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}>
              Documents
            </NavLink>
            <NavLink to="/notifications" className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}>
              Notifications
            </NavLink>
            {isManager ? (
              <div className="mt-3 border-t border-slate-200 pt-3">
                <p className="px-3 text-xs font-semibold uppercase tracking-wide text-slate-400">Manager</p>
                <NavLink
                  to="/manager/team-members"
                  className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}
                >
                  Team members
                </NavLink>
                <NavLink
                  to="/manager/team-expenses"
                  className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}
                >
                  Team expenses
                </NavLink>
              </div>
            ) : null}
            {isHr ? (
              <div className="mt-3 border-t border-slate-200 pt-3">
                <p className="px-3 text-xs font-semibold uppercase tracking-wide text-slate-400">HR</p>
                <NavLink
                  to="/hr/reviews"
                  className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}
                >
                  Expense reviews
                </NavLink>
                <NavLink
                  to="/hr/expense-config"
                  className={({ isActive }) => `${linkBase} ${isActive ? linkActive : ''}`}
                >
                  Expense configuration
                </NavLink>
              </div>
            ) : null}
          </nav>
        </aside>

        <main className="min-w-0 flex-1">
          <Outlet />
        </main>
      </div>
    </div>
  )
}