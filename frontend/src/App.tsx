import { RouterProvider } from "react-router-dom";
import { router } from "./routes/Routes";
import { LoginPage } from "./pages/LoginPage";

const App = () => <RouterProvider router={router} />;

// const App = () => <LoginPage />;

export default App;
