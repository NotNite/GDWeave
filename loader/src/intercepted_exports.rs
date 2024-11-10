#[allow(unused_imports)]
use proxygen_macros::{forward, post_hook, pre_hook, proxy};

use crate::start;

static mut STARTED: bool = false;

#[pre_hook(sig = "unknown")]
#[export_name = "timeBeginPeriod"]
pub extern "C" fn timeBeginPeriod() {
    if STARTED {
        return;
    }
    STARTED = true;
    start();
}
