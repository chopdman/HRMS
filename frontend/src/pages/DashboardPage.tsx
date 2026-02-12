import { Link } from 'react-router-dom'
import { Card } from '../components/ui/Card'
import { Header } from '../components/Header'
import { StatCard } from '../components/ui/StatCard'
import { useAuth } from '../hooks/useAuth'
import { useHrExpenses } from '../hooks/travel/useExpenses'
import { useNotifications } from '../hooks/useNotifications'

export const DashboardPage = () => {
  const { role } = useAuth()
  const isHr = role === 'HR'
  const hrExpenses = useHrExpenses({ status: 'Submitted' }, isHr)
  const pendingCount = hrExpenses.data?.length ?? 0
  const notifications = useNotifications()
  const unreadCount = notifications.data?.filter((item) => !item.isRead).length ?? 0

  return (
    <section className="space-y-6">
      <Header
        title="Overview"
        description="Monitor travel plans, expense activity, and notifications in one place."
        action={
          <Link
            className="inline-flex items-center justify-center rounded-md bg-brand-600 px-4 py-2 text-sm font-semibold text-white hover:bg-brand-700"
            to="/travels"
          >
            View travels
          </Link>
        }
      />

      <div className="grid gap-4 md:grid-cols-3">
        <StatCard label="Assigned travels" value="—"  />
       {isHr && <StatCard
          label="Expenses awaiting review"
          value={isHr ? String(pendingCount) : '—'}
          message={isHr ? 'Submitted expenses pending HR review.' : 'HR visibility only.'}
        />}
        <StatCard label="Unread notifications" value={String(unreadCount)}  />
      </div>


      {isHr ? (
        <Card className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <h3 className="text-base font-semibold text-slate-900">Pending HR reviews</h3>
            <p className="text-sm text-slate-500">{pendingCount} expenses waiting for action.</p>
          </div>
          <Link
            className="inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800"
            to="/hr/reviews"
          >
            Review now
          </Link>
        </Card>
      ) : null}

      <Card className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h3 className="text-base font-semibold text-slate-900">Notifications</h3>
          <p className="text-sm text-slate-500">{unreadCount} unread updates.</p>
        </div>
        <Link
          className="inline-flex items-center justify-center rounded-md border border-slate-200 px-4 py-2 text-sm font-semibold text-slate-700 hover:border-brand-200"
          to="/notifications"
        >
          View notifications
        </Link>
      </Card>
    </section>
  )
}